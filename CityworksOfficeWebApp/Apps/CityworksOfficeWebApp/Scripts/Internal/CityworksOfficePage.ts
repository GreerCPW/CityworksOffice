import { BasicPage } from "@jasonbenfield/sharedwebapp/Components/BasicPage";
import { CityworksOfficeAppClient } from "../Lib/Http/CityworksOfficeAppClient";
import { Apis } from "./Apis";
import { CityworksOfficePageView } from "./CityworksOfficePageView";

export class CityworksOfficePage extends BasicPage {
    protected readonly cwOfficeClient: CityworksOfficeAppClient;

    constructor(view: CityworksOfficePageView) {
        const apis = new Apis(view.modalError);
        const cwOfficeClient = apis.CityworksOffice();
        super(cwOfficeClient, view);
        this.cwOfficeClient = cwOfficeClient;
    }
}