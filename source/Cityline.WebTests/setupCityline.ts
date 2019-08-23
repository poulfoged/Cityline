import { CitylineClient, Frame } from "cityline-client";

const client = new CitylineClient(`${location.href}cityline`);
    
const outputElement = document.querySelector("[name=output]") as HTMLElement;

(async () => {
    const initialData = await client.getFrames("environment", "ping");
    console.log("We got initial", initialData);
})();

(async () => {
    const env = await client.getFrame<any>("environment");
    console.log("We got environment", env);
})();

client.addEventListener("frame-received", (event: CustomEvent<Frame>) => { outputElement.innerHTML += (`Got ${event.detail.event} ${JSON.stringify(event.detail.data)}<br />`); });
client.addEventListener("error", (event: CustomEvent<any>) => { console.log("Got error", event.detail); });