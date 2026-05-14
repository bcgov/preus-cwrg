app.controller('SystemSettingsManagement', function ($scope, $attrs, $controller, $timeout, Utils, ngDialog) {
  $scope.section = {
    name: 'SystemSettingsManagement',
    save: {
      url: '/Int/Admin/SystemSettings/Settings',
      method: 'PUT',
      data: function () {
        return $scope.model;
      },
      backup: true
    },
    onSave: function () {
      window.location = '/Int/Admin/SystemSettings/View';
    }
  };

  angular.extend(this, $controller('ParentSection', { $scope, $attrs }));

  function loadSettings() {
    return $scope.load({
      url: '/Int/Admin/SystemSettings/Settings',
      set: 'model'
    });
  }

  function init() {
    return Promise.all([
        loadSettings()
      ])
      .catch(angular.noop);
  }

  init();
});
