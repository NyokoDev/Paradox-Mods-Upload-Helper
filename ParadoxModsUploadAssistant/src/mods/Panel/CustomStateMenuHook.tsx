import { useState } from "react";

// Create a custom hook to manage the state
export const useCustomMenuState = () => {
  const [isOpened, setIsOpened] = useState(false);

  return { isOpened, setIsOpened };
};
