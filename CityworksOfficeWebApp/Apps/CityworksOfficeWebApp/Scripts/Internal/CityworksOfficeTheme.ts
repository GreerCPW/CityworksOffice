import { ButtonCommandView } from "@jasonbenfield/sharedwebapp/Views/Command";
import { ContextualClass } from "@jasonbenfield/sharedwebapp/ContextualClass";
import { ToolbarView } from "@jasonbenfield/sharedwebapp/Views/ToolbarView";
import { PaddingCss } from "@jasonbenfield/sharedwebapp/PaddingCss";
import { TextCss } from "@jasonbenfield/sharedwebapp/TextCss";
import { GridCellView } from "@jasonbenfield/sharedwebapp/Views/Grid";
import { BlockView } from "@jasonbenfield/sharedwebapp/Views/BlockView";

export class CityworksOfficeTheme {
    public static readonly instance = new CityworksOfficeTheme();

    mainContent(cell: GridCellView) {
        return cell
            .configure(c => c.positionRelative())
            .addView(BlockView)
            .configure(b => {
                b.positionAbsoluteFill();
                b.scrollable();
            })
            .addView(BlockView)
            .configure(b => {
                b.addCssName('container');
                b.setPadding(PaddingCss.top(3));
            });
    }

    readonly listItem = {
        deleteButton(button: ButtonCommandView) {
            return button
                .configure(b => {
                    b.icon.solidStyle('times');
                    b.icon.addCssFrom(new TextCss().context(ContextualClass.danger).cssClass());
                    b.useOutlineStyle(ContextualClass.secondary);
                    b.setText('');
                    b.setTitle('Delete');
                });
        }
    }

    readonly cardHeader = {
        editButton(button: ButtonCommandView) {
            return button
                .configure(b => {
                    b.icon.solidStyle('edit');
                    b.useOutlineStyle(ContextualClass.primary);
                    b.setText('Edit');
                    b.setTitle('Edit');
                });
        },
        addButton(button: ButtonCommandView) {
            return button
                .configure(b => {
                    b.icon.solidStyle('plus');
                    b.useOutlineStyle(ContextualClass.primary);
                    b.setText('Add');
                    b.setTitle('Add');
                });
        }
    }

    readonly commandToolbar = {
        toolbar(toolbar: ToolbarView) {
            return toolbar
                .configure(t => {
                    t.setBackgroundContext(ContextualClass.secondary);
                    t.addCssName('bg-opacity-25');
                    t.setPadding(PaddingCss.xs(3));
                });
        },
        backButton(button: ButtonCommandView) {
            return button
                .configure(b => {
                    b.icon.solidStyle('caret-left');
                    b.setText('Back');
                    b.setTitle('Back');
                    b.setContext(ContextualClass.light);
                });
        },
        cancelButton(button: ButtonCommandView) {
            return button
                .configure(b => {
                    b.icon.solidStyle('times');
                    b.setText('Cancel');
                    b.setTitle('Cancel');
                    b.setContext(ContextualClass.danger);
                });
        },
        saveButton(button: ButtonCommandView) {
            return button
                .configure(b => {
                    b.icon.solidStyle('check');
                    b.setText('Save');
                    b.setTitle('Save');
                    b.setContext(ContextualClass.primary);
                });
        }
    };
}