app.controller('ParticipantFundingSourcesItems', function ($scope, $attrs, $controller, $timeout, Utils) {
  $scope.section = {
    name: 'ParticipantFundingSourcesItems',
    save: {
      url: function () {
        return '/Int/Admin/ParticipantFundingSources/FundingSources';
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

  function loadFundingSources() {
    return $scope.load({
      url: '/Int/Admin/ParticipantFundingSources/FundingSources',
      set: 'model'
    });
  }

  function init() {
    return Promise.all([
        loadFundingSources()
      ])
      .catch(angular.noop);
  }

  init();

  $scope.createFundingStream = function () {
    if ($scope.model.FundingStreams == null)
      $scope.model.FundingStreams = [];

    let nextSequence = $scope.model.FundingStreams.length + 1;

    $scope.model.FundingStreams.push({
      Id: 0,
      Caption: "",
      RowSequence: nextSequence
    });
    $scope.renumberFundingStreams(1);
  }

  $scope.renumberFundingStreams = function (increment) {
    $scope.model.FundingStreams.sort(function (a, b) { return a.RowSequence - b.RowSequence });
    for (var aIdx = 0; aIdx < $scope.model.FundingStreams.length; aIdx++) {
      $scope.model.FundingStreams[aIdx].RowSequence = (aIdx + 1) * increment;
    }
  }

  $scope.moveFundingStream = function (fundingStream, up) {
    // Renumber by 100's. To move up or down, simply add or subtract 101, then renumber.
    $scope.renumberFundingStreams(100);
    if (up === 1)
      fundingStream.RowSequence -= 101;
    else
      fundingStream.RowSequence += 101;

    $scope.renumberFundingStreams(1);
  }

  $scope.removeItem = function (fundingStream) {
    const index = $scope.model.FundingStreams.indexOf(fundingStream);

    if (index > -1)
      $scope.model.FundingStreams.splice(index, 1);

    $scope.renumberFundingStreams(1);
  }
});
