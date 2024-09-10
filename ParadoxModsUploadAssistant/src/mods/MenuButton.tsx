import { MenuButton } from "cs2/ui";
import { ModuleRegistryExtend } from "cs2/modding";

import MyComponent from "./Panel/PanelMain";
import { useCustomMenuState } from "./Panel/CustomStateMenuHook";

export const CustomMenuButton: ModuleRegistryExtend = (Component) => {
  return (props) => {
    const { isOpened, setIsOpened } = useCustomMenuState(); // Use the custom hook

    const { children, ...otherProps } = props || {};

    return (
      <Component {...otherProps}>
        <MenuButton
onSelect={() => setIsOpened(true)} // Set isOpened to true to show the modal
        >
          Upload
        </MenuButton>
        {isOpened ? (
          <MyComponent /> // Render MyComponent if isOpened is true
        ) : (
          children // Render children if isOpened is false
        )}
      </Component>
    );
  };
};
