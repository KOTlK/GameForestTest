#version 330

in vec2 fragTexCoord;
in vec4 fragColor;

uniform vec2  gridSize;
uniform vec2  cellSize;
uniform int   selectedCell = 0;
uniform vec2  borderWidth = vec2(0.03, 0.03);
uniform vec4  selectedCellColor;
uniform vec4  selectedCellBorderColor = vec4(1.0, 0.0, 0.0, 1.0);
uniform vec4  borderColor = vec4(0.0, 1.0, 0.0, 1.0);

out vec4 finalColor;

int getCellIndex(vec2 uv) {
    int x = int(uv.x * gridSize.x);
    int y = int(uv.y * gridSize.y);
    return x + y * int(gridSize.x);
}

void main() {
    int cellIndex = getCellIndex(fragTexCoord);

    vec2 f = abs(fract(fragTexCoord * (gridSize + borderWidth)));

    bool isSelected = cellIndex == selectedCell;
    bool isBorder = f.x <= borderWidth.x || f.y <= borderWidth.y;

    if (isSelected) {
        finalColor = isBorder ? selectedCellBorderColor : selectedCellColor;
    } else {
        finalColor = isBorder ? borderColor : fragColor;
    }
}