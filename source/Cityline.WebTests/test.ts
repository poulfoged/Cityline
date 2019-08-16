import { CitylineClient, CitylineRestProtocol } from "./CitylineClient";

const client = new CitylineClient()
    .configure(new CitylineRestProtocol("https://localhost:5001/cityline"));



(async () => {
    const environment = await client.latestFrame<EnvironmentInfo>("environment", true);
    console.log("We got env", environment);
})();

interface EnvironmentInfo {
    clientStarted: string;
    started: string;
    version: any;
}

client.addEventListener("ping", (event: CustomEvent<Ping>) => { console.log("Got ping", event.detail); });

client.addEventListener("random", (event: CustomEvent<Random>) => { console.log("Got random", event.detail); });

interface Ping {
    ping: string;
}

interface Random {
    nextResponseInSeconds: number;
}