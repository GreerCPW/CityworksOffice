import { TextComponent } from '@jasonbenfield/sharedwebapp/Components/TextComponent';
import { CityworksOfficePage } from '../CityworksOfficePage';
import { MainPageView } from './MainPageView';

class MainPage extends CityworksOfficePage {
    protected readonly view: MainPageView;

    constructor() {
        super(new MainPageView());
        new TextComponent(this.view.heading).setText('Home Page');
    }
}
new MainPage();