(function ($) {
    $.fn.cdk_easyAutocomplete = function (options) {
        return this.each(function () {
            if (options == null || options == undefined) {
                console.log("please define options");
                return;
            }
            else if (options.source == null || options.source == undefined || options.source == '') {
                console.log("please define source");
                return;
            }

            var spinner = $(options.inputField).closest('.form-control-box').find('.fa-spinner');

            var cache = {},
                cacheProp,
                requestTerm;

            $(this).easyAutocomplete({
                adjustWidth: false,
                url: function (value) {
                    if (options.beforefetch && typeof (options.beforefetch) == "function") {
                        options.beforefetch();
                    }

                    requestTerm = value.replace(/^\s\s*/, '').replace(/\s\s*$/, '').replace(/-/g, ' ').replace(/[^\+A-Za-z0-9 ]/g, '').toLowerCase();

                    var year = options.year;
                    if (year != null && year != undefined && year != '') {
                        year = year.val();
                    }
                    else {
                        year = '';
                    }

                    //cacheProp = requestTerm + '_' + year;                    

                    if (requestTerm.length > 0) {                        
                            var path;                            
                            path = "/api/search/autocomplete/car/?source=" + options.source + "&term=" + encodeURIComponent(requestTerm);                            
                            var par = '';                            
                            par += __getValue('size', options.resultCount);                            
                            if (par != null && par != undefined && par != '') {
                                par = par.slice(0, -1);
                                path += '&' + par;
                            }
                            return path;                                               
                    }
                },

                getValue: "result",
                
                sourceType: options.source,

                ajaxSettings: {
                    async: true,
                    type: "GET",
                    contentType: "application/json; charset=utf-8",
                    dataType: "json",
                    beforeSend: function () {
                        spinner.show();
                    },
                    success: function (jsonData) {
                        spinner.hide();

                        cache[cacheProp] = [];                        
                    }
                },                                
                list: {
                    maxNumberOfElements: options.resultCount,
                    onChooseEvent: function (event) {
                        options.click(event);
                    },                    
                    onLoadEvent: function () {
                        var suggestionResult = $(options.inputField).getItems();

                        if (options.afterFetch != null && typeof (options.afterFetch) == "function") {
                            options.afterFetch(suggestionResult, requestTerm);
                        }
                    }
                }
            });

            $(this).keyup(function () {
                if (options.keyup != undefined) {
                    options.keyup();
                }

                if ($(options.inputField).val().replace(/\s/g, '').length == 0 && options.onClear != undefined) {
                    options.onClear();

                    $(options.inputField).closest('.easy-autocomplete').find('ul').hide();                    
                }
            });

            $(this).focusout(function () {
                if (options.focusout != undefined) {
                    options.focusout();
                }
            });

            function __getValue(key, value) {
                if (value != null && value != undefined && value != '') {
                    return key + '=' + value + '&';
                }
                else {
                    return '';
                }
            }

        });
    };
}(jQuery));