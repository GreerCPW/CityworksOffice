// Generated code

import { AppClient } from "@jasonbenfield/sharedwebapp/Http/AppClient";
import { AppClientEvents } from "@jasonbenfield/sharedwebapp/Http/AppClientEvents";
import { AppClientQuery } from "@jasonbenfield/sharedwebapp/Http/AppClientQuery";
import { HomeGroup } from "./HomeGroup";
import { ReceivablesGroup } from "./ReceivablesGroup";


export class CityworksOfficeAppClient extends AppClient {
	constructor(events: AppClientEvents) {
		super(
			events, 
			'CityworksOffice', 
			pageContext.EnvironmentName === 'Production' || pageContext.EnvironmentName === 'Staging' ? 'V3' : 'Current'
		);
		this.Home = this.addGroup((evts, resourceUrl) => new HomeGroup(evts, resourceUrl));
		this.Receivables = this.addGroup((evts, resourceUrl) => new ReceivablesGroup(evts, resourceUrl));
	}
	
	readonly Home: HomeGroup;
	readonly Receivables: ReceivablesGroup;
}