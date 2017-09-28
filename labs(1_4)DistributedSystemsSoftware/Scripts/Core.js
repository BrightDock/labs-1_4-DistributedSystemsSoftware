'use strict'

var title;
var bar;
var chartsList = [];

// update modal window on close
$("#myModal").on("hidden.bs.modal", function () {
    $('.box').removeClass('is-error is-success');
    $('.box label[for="file"]').html("<strong>Выберете файл</strong><span class='box__dragndrop'> или перетащите сюда</span>.");
});

function loader() {
    
}

// create random id, i'm used it for creating new charts 
function makeID(count)
{
    var text = "";
    var possible = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

    for( var i=0; i < count; i++ )
        text += possible.charAt(Math.floor(Math.random() * possible.length));

    return text;
}

function getCookie(name) {
    var matches = document.cookie.match(new RegExp(
      "(?:^|; )" + name.replace(/([\.$?*|{}\(\)\[\]\\\/\+^])/g, '\\$1') + "=([^;]*)"
    ));
    return matches ? decodeURIComponent(matches[1]) : undefined;
}

function setCookie(name, value) {
    var date = new Date(new Date().getTime() + 60000 * 1000);
    document.cookie = name + "=" + value + "; path=/; expires=" + date.toUTCString();
}

function deleteCookie(name) {
    setCookie(name, "", {
        expires: -1
    })
}

// load subject on click on menu item
$(function () {
    $(".subjects > li > label").click(function (e) {
        var menuElem = $(this).parent();

        if (!menuElem.hasClass("oppened")) {
            menuElem.removeClass("show");
            $.ajax({
                type: "get",
                url: "/Home/" + menuElem.attr("subject"),
                contentType: false,
                processData: false,
                cashe: false,
                success: function (result) {
                    deleteCookie("file");
/*                    if (window.soundManager != undefined) {
                        window.soundManager.stopAll();
                        window.soundManager.destruct();
                        window.soundManager.destroySound("sound0");
                        window.soundManager.reboot();
                    }
                    else {
                    }*/
                    if ($(window).width() > 768) {
                        menuElem.parent("ul").find("li").removeClass("oppened");
                        //        $(this).find("ul").toggle("oppened");
                        menuElem.toggleClass("oppened");
                        $(".menu-selector").css({
                            top: menuElem.position().top,
                            height: menuElem.height()
                        });
                    }
                    else {
                        menuElem.focus;
                        menuElem.parent("ul").find("li").removeClass("oppened");
                        menuElem.toggleClass("oppened");
                    }
                    $("#content").empty();
                    $("#content").append(result);
                    if (title != undefined) {
                        document.title = title;
                    }
                },
                error: function (xhr, status, p3) {
                    $('#notifier').removeClass("hidden");
                    $('#notifier').html("Ошибка загрузки данных<br />Пожалуйста, обновите страницу");
                }
            });
        }
        else {
            menuElem.toggleClass("show");
        }
    });
});

// load first subject on the site in menu and stuck or unstack menu by window size
$(document).ready(function () {
    $(".subjects > li:first-child > label").trigger("click");
    if ($(window).width() > 767) {
        $(".left-sidebar").stick_in_parent({ offset_top: 30 });
    }
    else {
        $(".subjects").addClass("row");
        $(".subjects > li").addClass("col");
    }
});

// stuck/ unstuck menu, muve selector display active menu
$(window).resize(function () {
    if ($(window).width() > 767) {
        $(".subjects").removeClass("row");
        $(".subjects > li").removeClass("col");
        $(".menu-selector").css({
            "height": $(".subjects > li.oppened ul li.selected").outerHeight() > 0 ?
                $(".subjects > li.oppened ul li.selected").outerHeight() : $(".subjects > li.oppened").outerHeight(),
            "top": $(".subjects > li.oppened ul li.selected").length > 0 ?
                $(".subjects > li.oppened ul li.selected").position().top : $(".subjects > li.oppened").position().top
        });
        $(".left-sidebar").trigger("sticky_kit:detach");
        $(".left-sidebar").stick_in_parent({ offset_top: 30 });
    }
    else {
        $(".subjects").addClass("row");
        $(".subjects > li").addClass("col");
        $(".left-sidebar").trigger("sticky_kit:detach");
    }
});

// customizing modal window on opening in needed context
$("#myModal").on("show.bs.modal", function (event) {
    var button = $(event.relatedTarget); // Button that triggered the modal
    var text = button.data("whatever"); // Extract info from data-* attributes
    var action = button.data("href");
    var modal = $(this);
    modal.find("form#fileForm").attr("action", action);
    modal.find(".modal-title").text(text);

    if (button.data("subject") === "pris") {
        modal.find("#sDescription").show();
    }
    else {
        modal.find("#sDescription").hide();
    }
});

function addLoader(o){
    var object = 0;
    printObjectText = function () {
        alert(object.text());
    }
    /*    
                            <div class="loader">
                                <div class="loader__figure"></div>
                                <p class="loader__label">Загрузка</p>
                            </div>*/
} (0);

// signalR functions
$(function () {
    $('#chatBody').hide();
    $('#notifier').addClass("hidden");
    //    $('#loginBlock').show();
    var chat = $.connection.mainRHub;

    chat.client.addMessage = function (message, IDs, uID) {
        $('#chatBody').show();
        $("#message").append("<div>" + uID + "</div>");
        $("#message").append("<div>" + $.connection.hub.id + "</div>");
        if ($('#myModal').hasClass('show')) {
            $('#myModal').modal('hide');
        }
        $('#notifier').removeClass("hidden");
        $('#notifier').html(message + "<br />");
        setTimeout(function () {
            $('#notifier').addClass("hidden");
        }, 5000);
    };

    chat.client.songAdded = function (message) {
        if ($('#myModal').hasClass('show')) {
            $('#myModal').modal('hide');
        }
        $('#notifier').removeClass("hidden");
        $('#notifier').html(message + "<br />");
        setTimeout(function () {
            $('#notifier').addClass("hidden");
        }, 5000);
    };

    chat.client.processProgress = function (percentage) {
        if ($('.progressLoader').length > 0) {
            if (percentage < 1) {
                $(".labTail h6").slideUp(100);
                $(".LabConclusion").slideUp(100);
                $(".labLeft").empty();
                $(".labRight").empty();
                $(".LabConclusion").empty();
            }
            $('.progressLoader').removeClass("end");
            $('.progressLoader').slideDown(200);
            document.querySelector('#content .progressLoader').setAttribute('percent', percentage);
            var $ppc = $('.progressLoader'),
                percent = parseInt(percentage),
                deg = 360 * percent / 100;
            if (percent > 50) {
                $ppc.addClass('gt-50');
            }
            if (percent === 100) {
                $ppc.addClass("end");
                $ppc.delay(700).slideUp(100);
            }
            $('.ppc-progress-fill').css('transform', 'rotate(' + deg + 'deg)');
            $('.ppc-percents span').html(percent + '%');
        }
    };

    chat.client.calcResult = function (result) {
        if ($('.progressLoader').length > 0) {
            var data = JSON.parse(result);
            $(".labLeft").append("<div class='row flex-nowrap'><p class='col col-6 align-self-center mb-0'>" + data.message +
                "</p><p class='col align-self-stretch divider mb-0'></p>" + "<p class='col align-self-center mb-0'>" + data.value + "</p>" + "</div>");
        }
    };

    chat.client.calcConclusion = function (result, dataType) {
        if ($('.progressLoader').length > 0) {
            var data = JSON.parse(result);
            if (dataType === "row") {
                $(".LabConclusion").append("<div class='row pl-4 pr-4 LabConclusion'><p>" + data.conclusion + "</p></div>");
            }
            else if (dataType === "rowsTitle") {
                $(".LabConclusion").append("<div class='row pl-4 mt-3 mb-2 pr-4 dataRowsTitle'><h5>" + data.title + "</h5></div>");
            }
            $(".LabConclusion").slideDown(100);
        }
    };
    
    chat.client.Notify = function (message) {
        if ($('#myModal').hasClass('show')) {
            $('#myModal').modal('hide');
        }
        $('#notifier').removeClass("hidden");
        $('#notifier').html(message + "<br />");
        setTimeout(function () {
            $('#notifier').addClass("hidden");
        }, 5000);

        if ($("#pris").hasClass("oppened")) {
            $.ajax({
                type: "get",
                url: "/Home/getSongsList",
                contentType: false,
                processData: false,
                cashe: false,
                success: function (result) {
                    $("ul.sm2-playlist-bd").html(result);

                    $(".song-name").click(function (elem) {
                        if ($(this).closest("li").find(".song-discription").text() != "") {
                            $(this).closest("li").toggleClass("expanded");
                        }
                        return false;
                    });
                },
                error: function (xhr, status, p3) {
                    $('#notifier').removeClass("hidden");
                    $('#notifier').html("Ошибка обновления списка песен<br />Пожалуйста, обновите страницу");
                    /*                setTimeout(function () {
                                        $('#notifier').hide();
                                    }, 200);*/
                }
            });
        }
    };

    chat.client.addChart = function (data, title, chartType) {
        var chartContainer = document.createElement("div");
        var chartID = makeID(5);
        $(chartContainer).addClass("chart", chartID);
        $(chartContainer).attr("id", chartID);
        $(".labRight").append(chartContainer);

        var chart = new CanvasJS.Chart(chartID, {
            theme: "theme2",//theme1
            animationEnabled: true,
            zoomEnabled: true,
            axisX: {
                labelFontSize: 12
            },
            title: {
                text: title
            },   // change to true
            data: [
            {
                // Change type to "bar", "area", "spline", "pie",etc.
                type: chartType,
                titleFontFamily: "verdana",
                dataPoints: JSON.parse(data)
            },
            {
                // Change type to "bar", "area", "spline", "pie",etc.
                type: "spline",

                dataPoints: JSON.parse(data)
            }]
        });
        chartsList.push({ Chart: chart, chartId: chartID });

        $('.chart').hover(function () {
            var elem = $(this);
            var chart = chartsList.find(function (object) { return object.chartId === elem.attr("id") }).Chart;
            elem.addClass("chartHover");
            setTimeout(function () {
                chart.set("width", elem.width(), false);
                chart.set("height", elem.height(), true);
            }, 90);
        }, function () {
            var elem = $(this);
            var chart = chartsList.find(function (object) { return object.chartId === elem.attr("id") }).Chart;

            elem.removeClass("chartHover");
            chart.set("width", elem.width(), false);
            chart.set("height", elem.height(), true);
            elem.removeAttr("style");
        });

        $(window).resize(function () {
            chartsList.forEach(function (chart) {
                var chartDiv = $('#' + chart.chartId);
                var parentDiv = chartDiv.parent();

                chart.Chart.set("width", parentDiv.width(), false);
                chart.Chart.set("height", chartDiv.height(), true);
            })
        });

        chart.render();
    };

    // Открываем соединение
    $.connection.hub.start().done(function () {
        chat.server.connect();
    });

    // Регистрируем пользовательски данные
    chat.client.onConnected = function () {
        setCookie("userID", $.connection.hub.id);
    };

    // Добавляем нового пользователя
    chat.client.onNewUserConnected = function (id, IDs) {
        /*        alert(id);
                $('#chatBody').show();
                $('#chatBody').append(id + " is added just now<br />");*/
    };

    // Удаляем пользователя
    chat.client.onUserDisconnected = function (id) {

    };
});

// The icon was borrowed from FlatIcon.
// Written by Osvaldas Valutis;
// Drag & Drop window for ajax loading files
(function (document, window, index) {

    // feature detection for drag&drop upload
    var isAdvancedUpload = function () {
        var div = document.createElement('div');
        return (('draggable' in div) || ('ondragstart' in div && 'ondrop' in div)) && 'FormData' in window && 'FileReader' in window;
    }();


    // applying the effect for every form
    var forms = document.querySelectorAll('.box');
    Array.prototype.forEach.call(forms, function (form) {
        var input = form.querySelector('input[type="file"]'),
            label = form.querySelector('label'),
            description = form.querySelector('.sDescription'),
            errorMsg = form.querySelector('.box__error span'),
            restart = form.querySelectorAll('.box__restart'),
            droppedFiles = false,
            showFiles = function (files) {
                label.textContent = files.length > 1 ? (input.getAttribute('data-multiple-caption') || '').replace('{count}', files.length) : files[0].name;
            },
            triggerFormSubmit = function () {
                var event = document.createEvent('HTMLEvents');
                event.initEvent('submit', true, false);
                form.dispatchEvent(event);
            };

        // letting the server side to know we are going to make an Ajax request
        var ajaxFlag = document.createElement('input');
        ajaxFlag.setAttribute('type', 'hidden');
        ajaxFlag.setAttribute('name', 'ajax');
        ajaxFlag.setAttribute('value', 1);
        form.appendChild(ajaxFlag);

        // automatically submit the form on file select
        input.addEventListener('change', function (e) {
            showFiles(e.target.files);


        });

        // drag&drop files if the feature is available
        if (isAdvancedUpload) {
            form.classList.add('has-advanced-upload'); // letting the CSS part to know drag&drop is supported by the browser

            ['drag', 'dragstart', 'dragend', 'dragover', 'dragenter', 'dragleave', 'drop'].forEach(function (event) {
                form.addEventListener(event, function (e) {
                    // preventing the unwanted behaviours
                    e.preventDefault();
                    e.stopPropagation();
                });
            });
            ['dragover', 'dragenter'].forEach(function (event) {
                form.addEventListener(event, function () {
                    form.classList.add('is-dragover');
                });
            });
            ['dragleave', 'dragend', 'drop'].forEach(function (event) {
                form.addEventListener(event, function () {
                    form.classList.remove('is-dragover');
                });
            });
            form.addEventListener('drop', function (e) {
                droppedFiles = e.dataTransfer.files; // the files that were dropped
                showFiles(droppedFiles);

            });
        }


        // if the form was submitted
        //        form.addEventListener('submit', function (e) {
        document.getElementById("SubmitBtn").addEventListener('click', function (e) {
            // preventing the duplicate submissions if the current one is in progress
            if (form.classList.contains('is-uploading')) return false;

            form.classList.add('is-uploading');
            form.classList.remove('is-error');

            if (isAdvancedUpload) // ajax file upload for modern browsers
            {
                e.preventDefault();

                // gathering the form data
                var ajaxData = new FormData(form);
                if (droppedFiles) {
                    Array.prototype.forEach.call(droppedFiles, function (file) {
                        ajaxData.append(input.getAttribute('name'), file);
                    });
                }

                // ajax request
                var subMenuElem = $("#oedCS ul > li.selected");
                var data = new FormData();
                data.append("file", document.getElementById("file").files[0]);
                data.append("SDescription", document.getElementById("sDescription").value);
                data.append("userID", getCookie('userID'));
                data.append("lab", (subMenuElem.attr("lr") + +subMenuElem.attr("number")));
//                alert(subMenuElem.attr("lr") + +subMenuElem.attr("number"));
                setCookie('file', document.getElementById("file").files[0].name.replace(/^.*[\\\/]/, ''));
                $.ajax({
                    type: form.getAttribute('method'),
                    url: form.getAttribute("action"),
                    contentType: false,
                    processData: false,
                    data: data,
                    success: function (result) {
                        form.classList.remove('is-uploading');
                        form.classList.add(result === "true" ? 'is-success' : 'is-error');

                    },
                    error: function (xhr, status, p3) {
                        form.classList.add('is-error');
                        form.classList.remove('is-uploading');
                        errorMsg.textContent = xhr.responseText;
                    }
                });
            }
            else // fallback Ajax solution upload for older browsers
            {
                var iframeName = 'uploadiframe' + new Date().getTime(),
                    iframe = document.createElement('iframe');

                $iframe = $('<iframe name="' + iframeName + '" style="display: none;"></iframe>');

                iframe.setAttribute('name', iframeName);
                iframe.style.display = 'none';

                document.body.appendChild(iframe);
                form.setAttribute('target', iframeName);

                iframe.addEventListener('load', function () {
                    var data = JSON.parse(iframe.contentDocument.body.innerHTML);
                    form.classList.remove('is-uploading')
                    form.classList.add(data.success === true ? 'is-success' : 'is-error')
                    form.removeAttribute('target');
                    if (!data.success) errorMsg.textContent = data.error;
                    iframe.parentNode.removeChild(iframe);
                });
            }
        });


        // restart the form if has a state of error/success
        Array.prototype.forEach.call(restart, function (entry) {
            entry.addEventListener('click', function (e) {
                e.preventDefault();
                form.classList.remove('is-error', 'is-success');
                description.textContent = "";
                input.click();
            });
        });

        // Firefox focus bug fix for file input
        input.addEventListener('focus', function () { input.classList.add('has-focus'); });
        input.addEventListener('blur', function () { input.classList.remove('has-focus'); });

    });
}(document, window, 0));