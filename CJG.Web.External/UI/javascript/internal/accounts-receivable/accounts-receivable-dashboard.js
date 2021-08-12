app.controller('AccountsReceivableDashboard', function ($scope, $attrs, $controller, $timeout, Utils) {
  $scope.section = {
    name: 'AccountsReceivableDashboard',
  };

  angular.extend(this, $controller('Section', { $scope, $attrs }));

  function loadFiscalYears() {
    return $scope.load({
      url: '/Int/Admin/AccountsReceivable/FiscalYears',
      set: 'fiscalYears',
      condition: !$scope.fiscalYears || !$scope.fiscalYears.length
    });
  }

  function loadAccountsReceivableDashboard() {
    return $scope.load({
      url: '/Int/Admin/AccountsReceivable/Data',
      set: 'model'
    });
  }

  function loadAccountsReceivableBreakdowns() {
    return $scope.load({
      url: '/Int/Admin/AccountsReceivable/Breakdowns?fiscalYearId=' + $scope.model.SelectedFiscalYearId,
      set: 'accountBreakdowns'
    });
  }

  $scope.refresh = function() {
    return $scope.ajax({
        url: '/Int/Admin/AccountsReceivable/Data?fiscalYearId=' + $scope.model.SelectedFiscalYearId,
        method: 'GET'
      })
      .then(function (response) {
        return $timeout(function() {
          loadAccountsReceivableBreakdowns();
          $scope.model = response.data;
        });
      })
      .catch(angular.noop);
  };

  $scope.breakdownTotal = function (accountBreakdown) {
    var total = 0;

    accountBreakdown.forEach(function(account) {
      const overpayment = parseFloat(account.Overpayment);
      total += overpayment;
    });

    return total;
  }

  /**
   * Fetch all the data for the form.
   * @function init
   * @returns {Promise}
   **/
  function init() {
    return Promise.all([
        loadFiscalYears(),
        loadAccountsReceivableDashboard()
      ])
      .then(function () {
        return $timeout(function () {
          loadAccountsReceivableBreakdowns();
        });
      })
      .catch(angular.noop);  }

  init();
});
