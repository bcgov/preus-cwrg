app.controller('ApplicationAccountsReceivables', function ($scope, $attrs, $controller, $timeout, Utils, ngDialog) {
  $scope.section = {
    name: 'ApplicationAccountsReceivables',
    displayName: 'Accounts Receivables',
    save: {
      url: '/Int/Application/AccountsReceivable',
      method: 'PUT',
      data: function () {
        return $scope.model;
      },
      backup: true
    },
    loaded: function () {
      return $scope.model
        && $scope.model.RowVersion
        && $scope.model.RowVersion === $scope.grantFile.RowVersion;
    },
    onSave: function () {
      $scope.emit('refresh', { target: 'ApplicationNotes', force: true });
    },
    onRefresh: function () {
      $scope.section.documents = [];
      return loadAR()
        .catch(angular.noop);
    },
    earliestAccountsReceivableDate: new Date(2018, 1, 1),
    latestAccountsReceivableDate: getFutureEndDate()
  };

  angular.extend(this, $controller('Section', { $scope: $scope, $attrs: $attrs }));

  /**
   * Make AJAX request to fetch attachment data
   * @function loadAttachments
   * @returns {Promise}
   **/
  function loadAR() {
    return $scope.load({
      url: '/Int/Application/AccountsReceivable/' + $scope.parent.grantApplicationId,
      set: 'model'
    });
  }

  function getFutureEndDate() {
    const d = new Date();
    const year = d.getFullYear();
    return new Date(year + 2, 12, 31);
  }

  $scope.rowTotal = function (records) {
    var total = 0;

    records.forEach(function (ar) {
      const overpayment = parseFloat(ar.Overpayment);
      total += overpayment;
    });

    return total;
  }

  $scope.init = function () {
    return Promise.all([
      loadAR()
    ]).catch(angular.noop);
  }
});
