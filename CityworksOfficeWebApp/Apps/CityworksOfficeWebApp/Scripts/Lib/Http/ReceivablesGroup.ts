// Generated code

import * as xti from "@jasonbenfield/sharedwebapp/Common";
import { AppClientGroup } from "@jasonbenfield/sharedwebapp/Http/AppClientGroup";
import { AppClientAction } from "@jasonbenfield/sharedwebapp/Http/AppClientAction";
import { AppClientView } from "@jasonbenfield/sharedwebapp/Http/AppClientView";
import { AppClientEvents } from "@jasonbenfield/sharedwebapp/Http/AppClientEvents";
import { AppResourceUrl } from "@jasonbenfield/sharedwebapp/Http/AppResourceUrl";

export class ReceivablesGroup extends AppClientGroup {
	constructor(events: AppClientEvents, resourceUrl: AppResourceUrl) {
		super(events, resourceUrl, 'Receivables');
		this.AddOrUpdateReceivablesAction = this.createAction<IEmptyRequest,IEmptyActionResult>('AddOrUpdateReceivables', 'Add Or Update Receivables');
	}
	
	readonly AddOrUpdateReceivablesAction: AppClientAction<IEmptyRequest,IEmptyActionResult>;
	
	AddOrUpdateReceivables(errorOptions?: IActionErrorOptions) {
		return this.AddOrUpdateReceivablesAction.execute({}, errorOptions || {});
	}
}