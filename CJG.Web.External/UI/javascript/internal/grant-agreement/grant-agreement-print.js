app.controller('GrantAgreementPrint', function ($scope, $attrs, $controller, $timeout) {
  $scope.section = {
    name: 'GrantAgreementPrint',
    //onRefresh: function () {
    //  return loadGrantAgreement().catch(angular.noop);
    //},
    grantApplicationId: $attrs.ngGrantApplicationId,
    version: $attrs.ngVersion
  };

  angular.extend(this, $controller('ParentSection', { $scope: $scope, $attrs: $attrs }));
  
  /**
   * Make AJAX request to load grant agreement data.
   * @function loadGrantAgreement
   * @returns {Promise}
   **/
  function loadGrantAgreement() {
    return $scope.load({
      url: '/Int/Application/Agreement/Print/Data/' + $scope.section.grantApplicationId + '/' + version,
      set: 'grantAgreement'
    });
  }

  /**
   * Fetch all the data for the form.
   * @function init
   * @returns {Promise}
   **/
  function init() {
    $('body').addClass("print-mode");

    return Promise.all([
        loadGrantAgreement()
      ]).then(function() {
        window.print();
      })
      .catch(angular.noop);
  }

  init();
});
