app.controller('GlobalBudgetDashboard', function ($scope, $attrs, $controller, $timeout, Utils) {
  $scope.section = {
    name: 'GlobalBudgetDashboard',
    save: {
      url: '/Int/Admin/GlobalBudget/Dashboard/Save',
      method: 'POST',
      data: function () {
        return $scope.model;
      }
    },
    onSave: function () {
      recalculateBudget();
    }
  };

  angular.extend(this, $controller('Section', { $scope, $attrs }));

  /**
   * Make AJAX request for fiscal years data
   * @function loadFiscalYears
   * @returns {Promise}
   **/
  function loadFiscalYears() {
    return $scope.load({
      url: '/Int/Admin/GlobalBudget/FiscalYears',
      set: 'fiscalYears',
      condition: !$scope.fiscalYears || !$scope.fiscalYears.length
    });
  }

  function loadBudgetDashboard() {
    return $scope.load({
      url: '/Int/Admin/GlobalBudget/Dashboard',
      set: 'model'
    });
  }

  function recalculateBudget() {
    $scope.recalculateBudget();
  };

  /**
   * Fetch all the data for the form.
   * @function init
   * @returns {Promise}
   **/
  function init() {
    return Promise.all([
        loadBudgetDashboard(),
        loadFiscalYears()
      ])
      .then(function () {
        return $timeout(function () {
          recalculateBudget();
        });
      })
      .catch(angular.noop);
  }

  $scope.recalculateBudget = function () {
    var existingCommitments = {};
    $scope.model.IntakePeriodSlots.forEach(function(slot) {
      slot.Streams.forEach(function(stream) {
        const intakeBudget = parseFloat(stream.StreamBudget);

        var previousOverUnderAllocation = existingCommitments[stream.GrantStreamId];
        if (previousOverUnderAllocation == undefined)
          previousOverUnderAllocation = 0;

        const overUnderAllocation = intakeBudget + previousOverUnderAllocation - stream.CommittedAmount;
        stream.OverUnderAllocation = overUnderAllocation;

        existingCommitments[stream.GrantStreamId] = overUnderAllocation;
      });
    });
  }

  /**
   * Make AJAX request to refresh data
   * @function refresh
   * @returns {Promise}
   **/
  $scope.refresh = function () {
    return $scope.ajax({
      url: '/Int/Admin/GlobalBudget/Dashboard?fiscalYearId=' + $scope.model.SelectedFiscalYearId,
      method: 'GET'
    })
      .then(function (response) {
        return $timeout(function () {
          $scope.model = response.data;
          recalculateBudget();
        });
      })
      .catch(angular.noop);
  };

  $scope.SlotSum = function(intakeSlot, propertyToSum) {
    let returnTotal = 0;
    intakeSlot.Streams.forEach((stream) => {
      let propertyTotal = +stream[propertyToSum];
      returnTotal += propertyTotal;
    });

    return returnTotal;
  };

  $scope.SlotPercentage = function (intakeSlot, topPropertyToSum, bottomPropertyToSum) {
    let topSlot = 0;
    let bottomSlot = 0;

    intakeSlot.Streams.forEach((stream) => {
      let propertyTop = +stream[topPropertyToSum];
      topSlot += propertyTop;

      let propertyBottom = +stream[bottomPropertyToSum];
      bottomSlot += propertyBottom;
    });

    if (bottomSlot === 0)
      return 0;

    return topSlot / bottomSlot;
  };

  $scope.GrandSum = function () {
    let returnTotal = 0;
    $scope.model.IntakePeriodSlots.forEach((slot) => {
      slot.Streams.forEach((stream) => {
        returnTotal += +stream['StreamBudget'];
      });
    });

    return returnTotal;
  };
  
  init();
});
