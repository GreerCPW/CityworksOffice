import { AppClientFactory } from "@jasonbenfield/sharedwebapp/Http/AppClientFactory";
import { ModalErrorView } from "@jasonbenfield/sharedwebapp/Views/ModalError";
import { CityworksOfficeAppClient } from "../Lib/Http/CityworksOfficeAppClient";

export class Apis {
    private readonly apiFactory: AppClientFactory;

    constructor(modalError: ModalErrorView) {
        this.apiFactory = new AppClientFactory(modalError)
    }

    CityworksOffice() {
        return this.apiFactory.create(CityworksOfficeAppClient);
    }
}