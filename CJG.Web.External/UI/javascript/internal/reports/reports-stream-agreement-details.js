app.controller('ReportsStreamAgreementDetails', function ($scope, $attrs, $controller) {
  $scope.section = {
    name: 'ReportsStreamAgreementDetails',
    fiscalYearId: $attrs.ngFiscalYearId
  };

  $scope.filter = {
    Keywords: null,
    CourseTitle: null,
    FiscalYearId: parseInt($attrs.ngFiscalYearId),
    GrantStreamId: null,
    NocId: null,
    RegionId: null,
    RegionIds: null,
    CommunityId: null,
    TrainingLocationName: null,

    OrderBy: []
  };

  angular.extend(this, $controller('ParentSection', { $scope: $scope, $attrs: $attrs }));

  function loadGrantStreams() {
    return $scope.load({
        url: '/Int/Admin/Reports/StreamAgreementDetails/Streams/' + ($scope.filter.FiscalYearId != null ? $scope.filter.FiscalYearId : 0),
        set: 'grantStreams'
      })
      .then(function() {
        $scope.selectedGrantStream = null;
      })
      .catch(angular.noop);
  }

  function loadFilterLookups() {
    return $scope.load({
        url: '/Int/Admin/Reports/StreamAgreementDetails/FilterLookups/',
        set: 'filterLookups'
      }).then(function(response) {
        loadGrantStreams();
      })
      .catch(angular.noop);;
  }

  function loadDataFilterLookups() {
    return $scope.load({
        url: '/Int/Admin/Reports/StreamAgreementDetails/DataFilterLookups/' + ($scope.filter.FiscalYearId != null ? $scope.filter.FiscalYearId : 0),
        set: 'dataFilterLookups'
      })
      .catch(angular.noop);;
  }

  function loadRegions() {
    return $scope.load({
      url: '/Int/Admin/Reports/StreamAgreementDetails/Regions/',
      set: 'regions'
    });
  }

  function loadCommunities() {
    return $scope.load({
      url: '/Int/Admin/Reports/StreamAgreementDetails/Communities/',
      set: 'communities'
    });
  }

  function loadTrainingLocations() {
    return $scope.load({
      url: '/Int/Admin/Reports/StreamAgreementDetails/TrainingLocations/' + ($scope.filter.FiscalYearId != null ? $scope.filter.FiscalYearId : 0),
      set: 'trainingLocations'
    });
  }

  function loadNOCs() {
    return $scope.load({
      url: '/Int/Admin/Reports/StreamAgreementDetails/NOCs/',
      set: 'nocs'
    });
  }

  $scope.onFiscalYearChange = function () {
    $scope.showIntakePeriods = false;
    $scope.showGrantStreamSelection = false;

    loadDataFilterLookups();
    loadGrantStreams();
    loadTrainingLocations();
  }

  $scope.refresh = function() {
    $("html").scrollTop(0);
    $(".reports--scrolling-container").scrollLeft(0);
    $(".reports--scrolling-container").scrollTop(0);
    return loadApplications();
  }

  /**
 * Apply the filter and load the applications.
 * @function applyFilter
 * @param {int} [page] - The page number.
 * @param {int} [quantity] - The number of items per page.
 * @param {bool} [force=false] - Whether to force a refresh.
 * @returns {Promise}
 */
  $scope.applyFilter = function (page, quantity, force) {
    //if (!page) page = $scope.filter.Page;
    //if (!quantity) quantity = $scope.filter.Quantity;
    //if (typeof (force) === 'undefined') force = false;

    //if (force || filterChanged()) $scope.cache = [];
    $("html").scrollTop(0);
    $(".reports--scrolling-container").scrollLeft(0);
    $(".reports--scrolling-container").scrollTop(0);

    return loadApplications()
      .catch(angular.noop);
  }

  /**
 * Make AJAX request to load applications.
 * @function loadApplications
 * @returns {Promise}
 **/
  function loadApplications() {
//    if (!page) page = 1;
    //if (!quantity) quantity = $scope.quantities[0];
    console.log($scope.filter);
    return $scope.load({
      url: '/Int/Admin/Reports/StreamAgreementDetails/Applications/',
      method: 'POST',
      data: $scope.filter,
      set: 'model'
    });
  }

  $scope.exportToExcel = function () {
    var filters = JSON.stringify($scope.filter);//.replace('#', '');
    window.open('/Int/Admin/Reports/StreamAgreementDetails/Applications/Export?filter=' + filters);
  };

  /**
   * Get the sorting order of the specified property.
   * @function sortDirection
   * @param {any} propertyName - The property name to order by.
   * @returns {string}
   */
  $scope.sortDirection = function (propertyName) {
    if (!isOrderedBy(propertyName))
      return 'sorting';

    return isAscending(propertyName) ? 'sorting_asc' : 'sorting_desc';
  }

  /**
   * Check if the filter is currently ordered by the specified property name.
   * @function isOrderedBy
   * @param {string} propertyName - The property name to order by.
   * @returns {bool}
   */
  function isOrderedBy(propertyName) {
    return $scope.filter.OrderBy.find(function (prop) { return prop.startsWith(propertyName); }) ? true : false;
  }

  /**
   * Check if the filter is currently be ordered in ascending order by the specified property name.
   * Use this to determine if the order by is ascending or descending.
   * @function isAscending
   * @param {string} propertyName - The property name to order by.
   * @returns {bool}
   */
  function isAscending(propertyName) {
    var found = $scope.filter.OrderBy.find(function (prop) { return prop.startsWith(propertyName); });
    if (!found)
      return true;

    return found.endsWith('desc') ? false : true;
  }

  /**
   * Order the applications by the specified property name.
   * @function sort
   * @param {string} propertyName - The property name to order by.
   * @returns {Promise}
   */
  $scope.sort = function (propertyName) {
    $scope.filter.OrderBy = [!isOrderedBy(propertyName) || !isAscending(propertyName) ? propertyName : propertyName + ' desc'];
    $scope.cache = [];

    return $scope.applyFilter();
  }

  $scope.search = function ($event) {
    if ($event.keyCode === 13)
      return $scope.applyFilter();

    return Promise.resolve();
  }

  function init() {
    return Promise.all([
        loadFilterLookups(),
        loadDataFilterLookups(),
        loadRegions(),
        loadCommunities(),
        loadTrainingLocations(),
        loadNOCs(),
        //loadGrantStreams(),
        loadApplications()
      ])
      .catch(angular.noop);
  }

  init();
});
