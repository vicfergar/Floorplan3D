import { useEvergineStore, EvergineCanvas } from "evergine-react";
import { EVERGINE_CANVAS_ID } from "./evergine/config";
import { useContainerSize } from "./evergine/useContainerSize";
import { useRef } from "react";
import './App.css'

function App() {
    const { webAssemblyLoaded, evergineReady } = useEvergineStore();
    const canvasContainer = useRef<HTMLDivElement>(null) as React.RefObject<HTMLDivElement>;

    return (
        <div className="App">
            {(!webAssemblyLoaded || !evergineReady) && (<div className="loading">Loading Evergine...</div>)}
            <div className="canvas-container" ref={canvasContainer}>
                <EvergineCanvas
                    canvasId={EVERGINE_CANVAS_ID}
                    width={useContainerSize(canvasContainer).width}
                    height={useContainerSize(canvasContainer).height}
                />
            </div>
        </div>
    );
}

export default App;
