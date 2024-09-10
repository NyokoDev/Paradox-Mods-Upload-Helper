import { ModRegistrar } from "cs2/modding";
import { HelloWorldComponent } from "mods/hello-world";
import { CustomMenuButton } from "mods/MenuButton";

const register: ModRegistrar = (moduleRegistry) => {
    moduleRegistry.extend(
        "game-ui/menu/components/main-menu-screen/main-menu-screen.tsx",
        "MainMenuNavigation",
        CustomMenuButton
    ); // Add this closing parenthesis and semicolon
}

export default register;
