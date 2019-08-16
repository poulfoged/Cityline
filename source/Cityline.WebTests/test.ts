import { CitylineClient, SkycityRestProtocol } from "./CitylineClient";

const client = new CitylineClient()
    .configure(new SkycityRestProtocol("https://localhost:5001/cityline"), true);

