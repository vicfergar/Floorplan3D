#!/bin/bash
set -e

EMSDK_VERSION=$2
INTERMEDIATE_PATH=$1

echo "Setup Emscripten SDK"
echo "Based on Uno WebAssembly Bootstrapper emsdk setup file"

echo "INTERMEDIATE_PATH: $INTERMEDIATE_PATH"
echo "EMSDK_VERSION: $EMSDK_VERSION"

echo Validating Mono Version
mono --version

echo Validating MSBuild Version
msbuild /version

export EMSDK_PATH=$INTERMEDIATE_PATH/emsdk-$EMSDK_VERSION

echo "EMSDK_PATH: $EMSDK_PATH"

if [ ! -f "$EMSDK_PATH" ]; then
	mkdir -p "$EMSDK_PATH"
fi

pushd "$EMSDK_PATH"

if [ ! -f .emsdk-install-done ]; then

	echo "Installing emscripten $EMSDK_VERSION in $EMSDK_PATH"

	git clone --branch $EMSDK_VERSION https://github.com/emscripten-core/emsdk 2>&1
	cd emsdk
	./emsdk install $EMSDK_VERSION
	./emsdk activate --embedded $EMSDK_VERSION

	# Those two files need to follow the currently used build of mono
	wget https://raw.githubusercontent.com/mono/mono/b777471fcace85325e2c2af0e460f4ecd8059b5a/sdks/builds/fix-emscripten-8511.diff 2>&1

	pushd upstream/emscripten
	patch -N -p1 < ../../fix-emscripten-8511.diff
	popd

	touch "$EMSDK_PATH/.emsdk-install-done"
else
	echo "Skipping installed emscripten $EMSDK_VERSION in $EMSDK_PATH"
fi
