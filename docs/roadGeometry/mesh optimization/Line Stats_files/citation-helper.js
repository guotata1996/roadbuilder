StackExchange.citationHelper = (function () {

    var currentResult = false;

    return { init: init }; // Hoisting!

    function thingFinder(markdown, textareaContent, sentinel, thingName, things) {
        return markdown;
    }

    function init() {
        if (StackExchange.externalEditor) {
            StackExchange.externalEditor.init({
                thingName: 'cite',
                thingFinder: thingFinder,
                buttonTooltip: 'Insert Citation',
                buttonImageUrl: '/content/Sites/mathoverflow/img/cite-icon.svg',
                onShow: function (editorClosed) {
                    $('#search-text').on('blur', runSearch).on('keypress', runSearchKey).focus();
                    $('#backlink').on('click', goBack);
                    currentResult = false;
                    $('#popup-cite .popup-submit').on('click', function () { if (currentResult) { listenMessage(currentResult); } });
                    window.addCitationHelper = editorClosed;

                    if (window.addEventListener) {
                        window.addEventListener('message', listenMessage, false);
                    } else {
                        window.attachEvent('onmessage', listenMessage);
                    }
                },
                onRemove: function () {
                    window.addCitationHelper = null;
                    try {
                        delete window.addCitationHelper;
                    } catch (e) { } // IE doesn't allow deleting from window
                },
                getDivContent: function (oldContent) {
                    return searchDialog();
                }
            });
        }
    }

    // Prepare the search dialog
    function searchDialog() {

        if ($('.popup-cite').length > 0) { return; } // Abort if dialog already exists

        // Tweaked version of SE close popup code. See popup.html for unminified HTML, genblob.sh can easily generate the below line from popup.html
        var popupHTML = '<div id="popup-cite" class="popup"><div class="popup-close"><a title="close this popup (or hit Esc)" href="javascript:void(0)">&times;</a></div><h2 class="popup-title-container handle"> <span class="popup-breadcrumbs"></span><span class="popup-title">Insert citation</span></h2><div id="pane-main" class="popup-pane popup-active-pane close-as-duplicate-pane" data-title="Insert Citation" data-breadcrumb="Cite"><input id="search-text" type="text" style="width: 740px; z-index: 1; position: relative;"><div class="search-errors search-spinner"></div> <div class="original-display"> <div id="previewbox" style="display:none"><div><a href="javascript:void(0)" id=backlink>&lt; Back to results</a></div><div class="preview" ></div></div> <div class="list-container"> <div class="list-originals" id="results"> </div> </div> </div></div><div class="popup-actions"><input type="submit" id="cite-submit" class="popup-submit disabled-button" value="Insert Citation" disabled="disabled" style="cursor: default;"></div></div>';

        return popupHTML;
    }

    // The event handler for the message
    function listenMessage(msg) {
        window.addCitationHelper(getCitationHtml(msg), '\n\n' + getCitationHtml(msg));
        StackExchange.MarkdownEditor.refreshAllPreviews();
        StackExchange.helpers.closePopups();
    }

    function runSearchKey(e) {
        var key = (e.keyCode ? e.keyCode : e.which);
        if (key == 13) {
            runSearch();
        }
    }

    // Run a search
    function runSearch() {
        goBack();
        $('#popup-cite .search-spinner').removeSpinner().addSpinner();
        $.getJSON('https://zbmath.org/citationmatching/mathoverflow', { 'q': $('#search-text').val() }, fetchCallback);
    }

    // Callback to run when search completes
    function fetchCallback(response) {
        var html = $('<div class="list">');
        for (var i = 0; i < response.results.length; i++) {
            var result = response.results[i];
            var zbl = 'https://zbmath.org/?q=an:' + result.zbl_id;

            var link = result.links.length > 0 ? result.links[0] : '';
            var arxiv = '';
            for (var j = 0; j < result.links.length; j++) {
                arxiv = result.links[j].includes("arxiv") ? result.links[j] : arxiv;
            }
            var authors = sanitizeForDisplay(result.authors);
            var title = sanitizeForDisplay(result.title);
            var citationHtml = sanitizeForDisplay(result.source);

            var renderResult = $('<div class="item" style="float:none;padding:5px">')
                .html($('<div class="summary post-link" style="float:none;width:auto;font-weight:bold;">')
                    .text(title))
                .append('<br/>')
                .append($('<span class="body-summary" style="float:none"></span>')
                    .append(authors + '<br/>' + citationHtml + '<br/> Preview (opens in new tab): ')
                    .append(renderOptionalLink(link, 'article'))
                    .append(renderOptionalLink(zbl, 'zbmath'))
                    .append(renderOptionalLink(arxiv, 'arxiv'))
                )
                .click(loadResultCallback(link, result))
                .hover(function () { $(this).css('background-color', '#e6e6e6') }, function () { $(this).css('background-color', '#fff') });

            html.append(renderResult);
            renderResult.find('a').on('click', function (e) { e.stopPropagation(); });
        }

        $('#results').html('').append(html);
        MathJax.Hub.Queue(['Typeset', MathJax.Hub, 'results']);
        $('#popup-cite .search-spinner').removeSpinner();
    }

    function sanitizeForDisplay(html) {
        return StackExchange.MarkdownEditor.sanitizeHtml(html);
    }

    function renderOptionalLink(href, text) {
        if (href) {
            return $(sanitizeForDisplay($('<a>').attr('href', href).text(text + ' ').prop('outerHTML'))).attr('target', '_blank');
        } else {
            return '';
        }
    }

    function loadResultCallback(href, result) {
        return function (e) { e.preventDefault(); e.stopPropagation(); loadResult(e, href, result); return false; }
    }

    function loadResult(e, href, result) {
        $('#popup-cite .popup-submit').enable();
        currentResult = result;
        $('.list-container').hide();
        $('#popup-cite #previewbox').show();
        $('#popup-cite .preview').html($(e.target).closest('.item').html());
    }

    function goBack() {
        $('#popup-cite .search-spinner').removeSpinner();
        $('.list-container').show();
        $('#popup-cite #previewbox').hide();
        $('#popup-cite .popup-submit').disable();
    }

    function getCitationHtml(json) {
        var cite = $('<cite>').attr('authors', json.authors)
                  .append('_' + json.authors + '_, ')
                  .append((json.links.length > 0 ? '[**' + json.title + '**](' + encodeURI(json.links[0]) + ')' : json.title) + ', ')
                  .append(json.source + ' [ZBL' + json.zbl_id + '](https://zbmath.org/?q=an:' + encodeURI(json.zbl_id) + ')')
                  .append('.');

        var citeContainer = $('<span></span>').append(cite).html();

        return citeContainer;
    }
})(); //end function call

StackExchange.using('editor', function (){
    StackExchange.using('externalEditor', function (){
        StackExchange.citationHelper.init();
    });
});