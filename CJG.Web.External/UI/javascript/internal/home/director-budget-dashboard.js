app.controller('DirectorBudgetDashboard', function ($scope, $attrs, $controller, $timeout) {
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
      .then(function() {
        return $timeout(function() {
          $scope.recalculateBudget();
        });
      }).catch(angular.noop);
  }

  function Base64ToBytes(base64) {
    console.log(base64);
    var s = window.atob(base64);
    var bytes = new Uint8Array(s.length);
    for (var i = 0; i < s.length; i++) {
      bytes[i] = s.charCodeAt(i);
    }
    return bytes;
  };

  //$scope.export = function () {
  //  var data = JSON.stringify($scope.model);//.replace('#', '');
  //  window.open('/Int/Home/Director/Dashboard/Export?data=' + data);
  //};

  $scope.export = function () {
    return $scope.ajax({
      url: '/Int/Home/Director/Dashboard/Export',
      method: 'POST',
      data: $scope.model
    }).then(function (response) {
        const blob = new Blob([Base64ToBytes(response.data.FileData)], { type: response.data.FileType });
        const downloadUrl = URL.createObjectURL(blob);
        const a = document.createElement("a");
        a.href = downloadUrl;
        a.download = response.data.FileName;
        document.body.appendChild(a);
        a.click();
      })
      .catch(angular.noop);
  }

  $scope.onFiscalYearChange = function () {
    return Promise.all([
        loadDirectorDashboard()
      ])
      .then(function () {
        return $timeout(function () {
          $scope.recalculateBudget();
        });
      }).catch(angular.noop);
  }

  $scope.recalculateBudget = function() {
    console.log('Recalculating Budget');

    if ($scope.model.DirectorsReport == undefined)
      return;

    if ($scope.model.OpeningBudgetRows == undefined)
      return;

    if ($scope.model.ClosingBudgetRows == undefined)
      return;

    var data = $scope.model.DirectorsReport;
    var openingRows = $scope.model.OpeningBudgetRows;
    var closingRows = $scope.model.ClosingBudgetRows;

    console.groupCollapsed("Director Budget Calc");
    data.forEach(item => {

      var directorBudgetId = item.DirectorBudgetId;
      var directorBudgetValue = parseFloat(item.Budget);

      if (isNaN(directorBudgetValue))
        return;

      console.log("Budget", directorBudgetValue);
      console.log("NAN", isNaN(directorBudgetValue));

      let directorOpeningBudgets = 0.0;
      let directorClosingBudgets = 0.0;

      openingRows.forEach(opening => {
        console.log('OPR', opening.DirectorBudgetEntries);

        opening.DirectorBudgetEntries.forEach(entry => {
          if (entry.DirectorBudgetId !== directorBudgetId)
            return;

          if (entry.Budget == null)
            return;

          let entryBudget = parseFloat(entry.Budget);
          if (isNaN(entryBudget))
            return;

          directorOpeningBudgets += entryBudget;
          console.log("Budget Entry: ", entryBudget);
        });
      });

      closingRows.forEach(closing => {
        console.log('CLR', closing.DirectorBudgetEntries);

        closing.DirectorBudgetEntries.forEach(entry => {
          if (entry.DirectorBudgetId !== directorBudgetId)
            return;

          if (entry.Budget == null)
            return;

          let entryBudget = parseFloat(entry.Budget);
          if (isNaN(entryBudget))
            return;

          directorClosingBudgets += entryBudget;
          console.log("Budget Entry: ", entryBudget);
        });
      });

      let adjustedBudget = directorBudgetValue + directorOpeningBudgets;
      let availableBudget = adjustedBudget - item.DirectorsReportPartialAvailableBudget;
      let remainingBudget = availableBudget + directorClosingBudgets;

      item.DirectorsReportAdjustedBudget = adjustedBudget;
      item.DirectorsReportAvailableBudget = availableBudget;
      item.DirectorsReportRemainingBudget = remainingBudget;
    });

    console.groupEnd();
    //  console.log($scope.model.DirectorsReport);
    //  console.log($scope.model.OpeningBudgetRows);
    //  console.log($scope.model.ClosingBudgetRows);

  }

  $scope.sumRow = function(data, fieldName) {
    let sum = 0;
    if (data == undefined)
      return sum;

    data.forEach(item => {
      let columnValue = item[fieldName];
      columnValue = parseFloat(columnValue);

      if (!isNaN(columnValue))
        sum += columnValue;
    });

    return sum;
  }

  $scope.sumColumn = function(data, column) {
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
