namespace Cleangorod.Data.Migrations
{
    using Cleangorod.Data.Models;
    using Microsoft.AspNet.Identity.EntityFramework;
    using Microsoft.AspNet.Identity.Owin;
    using Microsoft.Owin.Security;
    using OfficeOpenXml;
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Hosting;

    internal sealed class Configuration : DbMigrationsConfiguration<ApplicationDbContext>
    {

        public ISecureDataFormat<AuthenticationTicket> AccessTokenFormat { get; private set; }


        private string MapPath(string filePath)
        {
            if (HttpContext.Current != null)
                return Path.Combine(HostingEnvironment.ApplicationPhysicalPath, filePath);

            var absolutePath = new Uri(Assembly.GetExecutingAssembly().CodeBase).AbsolutePath;
            var directoryName = Path.GetDirectoryName(absolutePath);
            var path = Path.Combine(directoryName, filePath.Replace('/', '\\'));

            return path;
        }

        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            ContextKey = "Cleangorod.Data.Models.ApplicationDbContext";
        }

        protected override void Seed(Cleangorod.Data.Models.ApplicationDbContext context)
        {
            SeedAsync(context).Wait();
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );
            //
        }

        protected async Task SeedAsync(Cleangorod.Data.Models.ApplicationDbContext context)
        {
            var options = new IdentityFactoryOptions<ApplicationUserManager>();
            var userManager = ApplicationUserManager.Create(options, context);
            var roleManager = new ApplicationRoleManager(new RoleStore<IdentityRole>(context));

            await roleManager.CreateAsync(new IdentityRole { Name = "Client" });
            await roleManager.CreateAsync(new IdentityRole { Name = "Admin" });
            await ImportUsers(userManager);
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data. E.g.
            //
            //    context.People.AddOrUpdate(
            //      p => p.FullName,
            //      new Person { FullName = "Andrew Peters" },
            //      new Person { FullName = "Brice Lambson" },
            //      new Person { FullName = "Rowan Miller" }
            //    );
            //
        }

        private async Task ImportUsers(ApplicationUserManager userManager)
        {
            var excelSrcPath = MapPath("Migrations/SeedData/Customers_14-15022015.xlsx");
            var csvSrcPath = MapPath("Migrations/SeedData/members_export_d88509a54a.csv");

            IEnumerable<ExternalUser> mergedUsers = null;

            using (var csvInputStream = File.OpenRead(csvSrcPath))
            {
                mergedUsers = GetMergedUsers(csvInputStream, File.OpenRead(excelSrcPath));
            }

            foreach (var item in mergedUsers)
            {
                await SaveExternalUser(userManager, item);
            }
        }

        private async Task SaveExternalUser(ApplicationUserManager userManager, ExternalUser externalUser)
        {
            var user = new ApplicationUser()
            {
                UserName = externalUser.Email,
                Email = externalUser.Email,
                PhoneNumber = externalUser.PhoneNumber,
                Name = externalUser.Name,
                Surname = externalUser.Surname,
                Note = externalUser.Note,
                ClientAddress = new ClientAddress
                {
                    Address = externalUser.Address,
                    Latitude = externalUser.Latitude,
                    Longitude = externalUser.Longitude,
                }
            };

            var result = await userManager.CreateAsync(user, "Moomoo/99");
            if (!result.Succeeded)
            {
                throw new Exception(String.Join("; ", result.Errors));
            }
            await userManager.AddToRoleAsync(user.Id, "Client");
        }

        private class ExternalUser
        {
            public string Email { get; set; }

            public double? Latitude { get; set; }

            public double? Longitude { get; set; }

            public string Name { get; set; }

            public string Surname { get; set; }

            public string PhoneNumber { get; set; }

            public string Note { get; set; }

            public string Address { get; set; }
        }

        private static IEnumerable<ExternalUser> GetExcelUsers(Stream inputStream)
        {
            using (var excelPackage = new ExcelPackage(inputStream))
            {
                var worksheet = excelPackage.Workbook.Worksheets.First();
                var start = worksheet.Dimension.Start;
                var end = worksheet.Dimension.End;

                int position;
                return Enumerable.Range(start.Row, end.Row - start.Row + 1)
                    .Select(x => (Func<string, ExcelRange>)((string col) => worksheet.Cells[col + x]))
                    .Where(x => int.TryParse(x("A").Text, out position))
                    .Select(x => new ExternalUser()
                    {
                        Email = x("G").Text,
                        Longitude = double.Parse(x("J").Text.Split(',')[0], CultureInfo.InvariantCulture),
                        Latitude = double.Parse(x("J").Text.Split(',')[1], CultureInfo.InvariantCulture),
                        Name = x("D").Text,
                        Surname = x("E").Text,
                        Note = x("AZ").Text,
                        PhoneNumber = x("F").Text,
                        Address = x("I").Text
                    })
                    .ToList();
            }
        }

        private static IEnumerable<ExternalUser> GetCsvUsers(Stream inputStream)
        {
            var rows = ReadCsv(inputStream);
            return rows.Skip(1).Select(x => new ExternalUser
            {
                Name = x[0],
                Surname = x[1],
                PhoneNumber = x[2],
                Email = x[3],
                Address = x[4],
            }).ToList();
        }

        private static IEnumerable<ExternalUser> GetMergedUsers(Stream csvStream, Stream excelStream)
        {
            var excelUsers = GetExcelUsers(excelStream);
            var csvUsers = GetCsvUsers(csvStream).ToList();

            var csvDict = csvUsers.ToDictionary(x => x.Email, x => x);
            foreach (var item in excelUsers)
            {
                if (!csvDict.ContainsKey(item.Email))
                {
                    csvUsers.Add(item);
                    continue;
                }
                var csvUser = csvDict[item.Email];
                csvUser.Latitude = item.Latitude;
                csvUser.Longitude = item.Longitude;
                csvUser.PhoneNumber = item.PhoneNumber;
                csvUser.Address = item.Address;
                csvUser.Note = item.Note;
            }

            return csvUsers;
        }

        private static void SaveMergedUsers(string csvScrPath, string excelSrcPath, string csvDestPath)
        {
            using (var excelInputStream = File.OpenRead(excelSrcPath))
            {
                var mergedUsers = GetMergedUsers(File.OpenRead(csvScrPath), excelInputStream);
                File.WriteAllLines(csvDestPath, mergedUsers.Select(x => String.Join(",", x)));
            }
        }

        private static List<string[]> ReadCsv(Stream inputStream)
        {
            List<string[]> list = new List<string[]>();
            using (var reader = new StreamReader(inputStream))
                while (!reader.EndOfStream)
                    list.Add(reader.ReadLine().Split(','));
            return list;
        }
    }
}
