import { useState, useLayoutEffect } from 'react';

export function useContainerSize(canvasContainer: React.RefObject<HTMLDivElement>) {

  const [containerSize, setContainerSize] = useState({
    width: 0,
    height: 0
  });

  useLayoutEffect(() => {
    function handleResize() {
        setContainerSize({
            width: canvasContainer.current?.clientWidth ?? 0,
            height: canvasContainer.current?.clientHeight ?? 0
        });
    }
    window.addEventListener('resize', handleResize);
    handleResize();
    return () => window.removeEventListener('resize', handleResize);
  }, [canvasContainer]);
  return containerSize;
}
