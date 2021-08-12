app.controller('GrantStreamChecklist', function ($scope, $attrs, $controller, $timeout, Utils) {
  $scope.section = {
    name: 'GrantStreamChecklist',
    save: {
      url: function () {
        return '/Int/Admin/Grant/Streams/UpdateChecklist';
      },
      method: 'PUT',
      data: function () {
        return $scope.model;
      },
      backup: true
    },
    onSave: function () {
      $scope.emit('refresh');
    },
    loaded: function () {
      return $scope.section.isLoaded;
    }
  };

  angular.extend(this, $controller('Section', { $scope: $scope, $attrs: $attrs }));

  $scope.$watch('$parent.model', function (newValue, oldValue) {
    $scope.model = newValue;
  });

  /**
 * Initialize the data for the form
 * @function init
 * @returns {void{
 **/
  $scope.init = function () {
    return Promise.resolve();
  }

  $scope.createCategory = function () {
    $scope.model.CheckListCategories.push({
      Id: 0,
      GrantStreamId: $scope.model.Id,
      Caption: "",
      IsActive: true,
      RowSequence: 0
    });
    $scope.renumberCategories(1);
  }

  $scope.createItem = function (category) {

    if (category.Items == null)
      category.Items = [];

    let nextSequence = category.Items.length + 1;

    category.Items.push({
      Id: 0,
      Caption: "",
      IsActive: true,
      RowSequence: nextSequence
    });
  }

  $scope.renumberCategories = function (increment) {
    $scope.model.CheckListCategories.sort(function (a, b) { return a.RowSequence - b.RowSequence });
    for (var aIdx = 0; aIdx < $scope.model.CheckListCategories.length; aIdx++) {
      $scope.model.CheckListCategories[aIdx].RowSequence = (aIdx + 1) * increment;
    }
  }

  $scope.renumberCategoryItems = function (category, increment) {
    category.Items.sort(function (a, b) { return a.RowSequence - b.RowSequence });
    for (var aIdx = 0; aIdx < category.Items.length; aIdx++) {
      category.Items[aIdx].RowSequence = (aIdx + 1) * increment;
    }
  }

  $scope.moveCategory = function (category, up) {
    // Renumber by 100's. To move up or down, simply add or subtract 101, then renumber.
    $scope.renumberCategories(100);
    if (up === 1)
      category.RowSequence -= 101;
    else
      category.RowSequence += 101;

    $scope.renumberCategories(1);
  }

  $scope.moveItem = function (category, item, up) {
    // Renumber by 100's. To move up or down, simply add or subtract 101, then renumber.
    $scope.renumberCategoryItems(category, 100);

    if (up === 1)
      item.RowSequence -= 101;
    else
      item.RowSequence += 101;

    $scope.renumberCategoryItems(category, 1);
  }

  $scope.removeItem = function (category, item) {
    const index = category.Items.indexOf(item);

    if (index > -1)
      category.Items.splice(index, 1);

    $scope.renumberCategoryItems(category, 1);
  }

  $scope.filterCategories = function (question) {
    return true;
  }
});
