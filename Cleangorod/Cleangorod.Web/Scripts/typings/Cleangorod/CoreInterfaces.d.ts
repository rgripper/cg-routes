interface ApplicationContext {
    identity: {
        isAuthenticated: boolean;
        name: string;
        roles: string[];
    }
}

interface IRouter {
    componentName: KnockoutObservable<string>;

    params: KnockoutObservable<{ get(name: string): string; }>;
}