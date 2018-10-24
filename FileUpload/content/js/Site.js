"use strict";

function installHoverPreview() {
    var links = document.querySelectorAll(".file-links a.img-link");
    var preview = document.querySelector("#preview-img");

    var _iteratorNormalCompletion = true;
    var _didIteratorError = false;
    var _iteratorError = undefined;

    try {
        for (var _iterator = links[Symbol.iterator](), _step; !(_iteratorNormalCompletion = (_step = _iterator.next()).done); _iteratorNormalCompletion = true) {
            var lnk = _step.value;

            lnk.addEventListener("mouseover", function (e) {
                preview.src = e.target.href;
                preview.style.display = 'block';
                preview.style.left = e.clientX + 10 + "px";
                preview.style.top = e.clientY + 10 + "px";
            });

            lnk.addEventListener("mouseout", function (e) {
                preview.style.display = 'none';
            });
        }
    } catch (err) {
        _didIteratorError = true;
        _iteratorError = err;
    } finally {
        try {
            if (!_iteratorNormalCompletion && _iterator.return) {
                _iterator.return();
            }
        } finally {
            if (_didIteratorError) {
                throw _iteratorError;
            }
        }
    }

    $(document).ready(function () {
        $('[data-role="tags-input"]').tagsInput();
    });
}

var oldOnload = window.onload;
window.onload = function () {
    // Call original onload if installed
    if (oldOnload) oldOnload.call(this);

    installHoverPreview();
};