import { initializeEvergineBase } from "evergine-react";

import {
  EVERGINE_ASSEMBLY_NAME,
  EVERGINE_CLASS_NAME,
  EVERGINE_LOADING_BAR_ID,
} from "../evergine/config";

declare global {
  let Blazor: { start(): Promise<void> };
}

const initializeEvergine = (): void => {
  initializeEvergineBase(
    EVERGINE_LOADING_BAR_ID,
    EVERGINE_ASSEMBLY_NAME,
    EVERGINE_CLASS_NAME
  );
};

export { initializeEvergine };
