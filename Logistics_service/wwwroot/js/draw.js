let points = {};

function FillButtons(model, color = 'green') {

    const imageContainer = document.getElementById('imageContainer');
    const buttons = document.querySelectorAll('.point-button');
    const containerRect = imageContainer.getBoundingClientRect();

    buttons.forEach(button => {
        const index = button.getAttribute('data-index');
        const rect = button.getBoundingClientRect();

        const x = ((rect.left - containerRect.left) / containerRect.width) * 100;
        const y = ((rect.top - containerRect.top) / containerRect.height) * 100;

        points[index] = { x, y };
        console.log('point' + index + '=(' + x + ',' + y + ');');
    });

    if (color == 'green') {
        model.forEach(point => {
            if (point.ConnectedPointsIndexes) {
                for (var i = 0; i < point.ConnectedPointsIndexes.length; i++) {
                    drawLine(point.index, point.ConnectedPointsIndexes[i], imageContainer, color);
                }
            }
        });
    } else {
        model.forEach(point => {
            if (point.ConnectedPointsIndexes) {
                point.ConnectedPointsIndexes.forEach(connectedIndex => {
                    const connectedPoint = model.find(p => p.index === connectedIndex);
                    if (connectedPoint) {
                        drawLine(point.index, connectedIndex, imageContainer, color);
                    }
                });
            }
        });
    }
}

function drawLine(p1, p2, imageContainer, color) {
    var line = document.createElement('div');
    line.className = 'connection-line';

    var modelElement = document.getElementById('model');
    if (modelElement) {
        var model = JSON.parse(modelElement.textContent);
        var point1 = model[p1];
        var point2 = model[p2];

        const width = imageContainer.offsetWidth;
        const height = imageContainer.offsetHeight;

        var deltaX = (point2.posX - point1.posX) * 0.01 * width;
        var deltaY = (point2.posY - point1.posY) * 0.01 * height;

        var angle = -Math.atan(deltaX / deltaY);

        if (point2.posY < point1.posY) {
            angle += Math.PI;
        }

        var length = Math.sqrt(deltaX * deltaX + deltaY * deltaY);

        line.id = p1 + '-' + p2;
        line.style.position = 'absolute';
        line.style.backgroundColor = color;
        line.style.width = '3px';
        line.style.height = `${length}px`;
        line.style.transformOrigin = 'top left';
        line.style.transform = `rotate(${angle}rad)`;
        line.style.left = `${point1.posX}%`;
        line.style.top = `${point1.posY}%`;
        line.style.zIndex = 100;

        imageContainer.appendChild(line);
    }
    else {
        console.error('Элемент с id="model" не найден');
    }
}

function addNewLine(pointIndex1, pointIndex2) {
    const imageContainer = document.getElementById('imageContainer');
    if (pointIndex1 && pointIndex2) {
        drawLine(pointIndex1, pointIndex2, imageContainer);
    } else {
        console.error('Одна из точек не найдена');
    }
}