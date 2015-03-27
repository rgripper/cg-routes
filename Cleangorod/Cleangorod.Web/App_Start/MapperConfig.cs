using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Google;
using Microsoft.Owin.Security.OAuth;
using Owin;
using Cleangorod.Web.Providers;
using Cleangorod.Web.Models;
using Cleangorod.Data.Models;
using Microsoft.AspNet.Identity.Owin;
using AutoMapper;

namespace Cleangorod.Web
{
    public static class MapperConfig
    {
        public static void CreateMappings()
        {
            Mapper.CreateMap<ClientSchedule, ClientScheduleViewModel>()
                .ForMember(x => x.Ranges, x => x.MapFrom(r => Mapper.Map<List<CollectionDateTimeRangeViewModel>>(r.Ranges)));
            //Mapper.CreateMap<CollectionScheduleViewModel, CollectionSchedule>().ForMember(x => x.Client, x => x.Ignore());
            
            Mapper.CreateMap<CollectionDateTimeRange, CollectionDateTimeRangeViewModel>().ForMember(x => x.Selected, x => x.Ignore());
            //Mapper.CreateMap<CollectionDateTimeRangeViewModel, CollectionDateTimeRange>();


        }
    }
}
