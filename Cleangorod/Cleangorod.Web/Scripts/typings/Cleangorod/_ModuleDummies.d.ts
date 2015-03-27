declare module "knockout" { } 

interface KnockoutObservable<T> {
    subscribeChanged: (handler: (newValue: T, oldValue: T) => any) => KnockoutSubscription;
}