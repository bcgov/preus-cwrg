app.controller('DirectorBudgetDashboard', function ($scope, $attrs, $controller) {
  $scope.section = {
    name: 'DirectorBudgetDashboard',
    save: {
      url: '/Int/Home/Director/Dashboard/Save',
      method: 'POST',
      data: function () {
        $scope.model.FiscalYearId = $scope.section.fiscalYearId;
        return $scope.model;
      }
    },
    budgetTotal: 0,
    fiscalYearId: parseInt($attrs.ngFiscalYearId),
    currentFiscalYearId: parseInt($attrs.ngFiscalYearId)
  };

  angular.extend(this, $controller('Section', { $scope, $attrs }));

  function loadFiscalYears() {
    return $scope.load({
        url: '/Int/Home/Director/FiscalYears',
        set: 'fiscalYears'
      })
      .then(function(response) {
      })
      .catch(angular.noop);
  }

  function loadDirectorDashboard() {
    return $scope.load({
      url: '/Int/Home/Director/Dashboard/' + $scope.section.fiscalYearId,
      set: 'model'
    });
  }
  
  /**
   * Fetch all the data for the form.
   * @function init
   * @returns {Promise}
   **/
  function init() {
    return Promise.all([
      loadFiscalYears(),
      loadDirectorDashboard()
    ])
    .catch(angular.noop);
  }

  $scope.onFiscalYearChange = function () {
    loadDirectorDashboard();
  }

  $scope.sumColumns = function(data, column) {
    let sum = 0;
    if (data == undefined)
      return sum;

    data.forEach(item => {
      let columnValue = item[column];
      columnValue = parseFloat(columnValue);

      if (!isNaN(columnValue))
        sum += columnValue;
    });

    return sum;
  }

  $scope.sumSlippage = function(data) {
    let sum = 0;
    if (data == undefined)
      return sum;

    data.forEach(item => {
      let columnValue = item['Budget'];
      let useInSlippage = item['IncludeInSlippageCalculation'];

      columnValue = parseFloat(columnValue);

      if (!isNaN(columnValue) && useInSlippage)
        sum += columnValue;
    });

    return sum * 1.05;
  }

  init();
});
