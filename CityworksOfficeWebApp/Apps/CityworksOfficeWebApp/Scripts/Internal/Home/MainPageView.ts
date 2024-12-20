import { BasicTextComponentView } from '@jasonbenfield/sharedwebapp/Views/BasicTextComponentView';
import { TextHeading1View } from '@jasonbenfield/sharedwebapp/Views/TextHeadings';
import { CityworksOfficePageView } from '../CityworksOfficePageView';

export class MainPageView extends CityworksOfficePageView {
    readonly heading: BasicTextComponentView;

    constructor() {
        super();
        this.heading = this.addView(TextHeading1View);
    }
}