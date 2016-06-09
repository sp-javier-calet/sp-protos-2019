#!/bin/bash

DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd)"

OUTPUT=$DIR/test_result.xml

/Applications/Unity/Unity.app/Contents/MacOS/Unity -batchMode -projectPath $DIR/../Project -runEditorTests -editorTestsResultFile $OUTPUT -quit

echo "Results available at "$OUTPUT