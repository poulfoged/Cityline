interface EventMap {
    "received": CustomEvent<any>;
    "error": CustomEvent<string>;
}

export class CitylineClient {
    private _protocol: ICitylineProtocol;
    private static _instance = new CitylineClient();
    private _initialize: Promise<void>;
    public static get instance() { return CitylineClient._instance; }
    private _frames: { [key: string]: IFrame } = {};
    private wait = ms => new Promise(r => window.setTimeout(r, ms));
    private _enableDebug;

    public configure(protocol: ICitylineProtocol, enableDebug = false) {
        if (!protocol)
            throw new Error("protocol must be provided.")

        this._enableDebug = enableDebug;
        this._protocol = protocol;
        setTimeout(async () => this.initialize());
        return this;
    }

    private log(statement: () => string) {
        if (!this._enableDebug)
            return;
        
        console.log(statement());
    }
    
    private initialize = () => {
        if (this._initialize)
            return this._initialize;

        this.log(() => "Initializing");

        this._initialize = new Promise(async r => {
            // wait for config
            while(!this._protocol) {
                this.log(() => "Waiting for protocol to be set");
                await this.wait(100);
            }
                
            this.log(() => "Starting listener");
            this._protocol.startListening((data) => {
                // wait for first successful callback
                this.log(() => "Got callback");
                this.callback(data);
                this.log(() => "Marking initialization complete");
                r(); // ok, we are ready
            }, this.errorCallback, this._enableDebug);
        });
    }

    async latestFrame<T>(name: string, throwError = true) {
        await this.initialize();
        const latest = this._frames[name];

        if (!latest && throwError)
            throw new Error(`No frame named ${name}.`);

        return latest;
    }

    private callback = (response: CitylineResponse) => {
        for(const key in response.carriages) {
            this.log(() => `Storing frame ${key} and raising event`);
            this._frames[name] = response.carriages[key]; 
            console.log("data", response.carriages[key]);
            // raise generic event
            // raise specific event
        }
    }

    private errorCallback = (error: string) => {}
}

export interface ICitylineProtocol {
    startListening(callback: (response: CitylineResponse) => void, errorCallback: (error: string) => void, enableDebug: boolean);
}

export class SkycityRestProtocol implements ICitylineProtocol {
    private _callback: (response: CitylineResponse) => void;
    private _errorCallback: (error: string) => void;
    private tickets: { [key: string]: string } = {};
    private _enableDebug: boolean;
    
    constructor(private server: string, private requestFactory: () => RequestInit = () => ({})) {}

    startListening(callback: (response: CitylineResponse) => void, errorCallback: (error: string) => void, enableDebug = false) {
        this._enableDebug = enableDebug;
        this._callback = callback;
        this._errorCallback = errorCallback;
        setTimeout(async () => this.startCycle());
    }

    private log(statement: () => string) {
        if (!this._enableDebug)
            return;
        
        console.log(statement());
    }

    private async startCycle() {
        try {
            var headers = new Headers();
            headers.append('Content-Type', 'application/json');

            const requestData: CitylineRequest = { tickets: this.tickets };
            const request: RequestInit = {...{ body: JSON.stringify(requestData), method: "post", headers: headers }, ...this.requestFactory()};
            const response = await fetch(this.server, request);
            const citylineResponse = await response.json() as CitylineResponse;

            this._callback(citylineResponse);

            for(const key in citylineResponse.carriages) 
                this.tickets[key] = citylineResponse.carriages[key].ticket;
        
        } catch (error) {
            this.log(() => "startCycle error" + error);
            
        } finally {
            setTimeout(async () => await this.startCycle(), 1000);
        }
    }

}

interface CitylineResponse {
    carriages: { [key: string]: IFrame }
}


interface CitylineRequest {
    tickets: { [key: string]: string }
}

interface IFrame {
    ticket: string;
    cargo: any;
}

