app.controller('ApplicationSummary', function ($scope, $attrs, $controller, $timeout, Utils, ngDialog) {
  $scope.section = {
    name: 'ApplicationSummary',
    displayName: 'Summary',
    save: {
      url: '/Int/Application/Summary',
      method: 'PUT',
      dataType: 'file',
      data: function () {
        $scope.model.SelectedDeliveryPartnerServiceIds = $scope.deliveryPartnerServices
          .filter(function (item) { return item.isChecked; })
          .map(function (item) {
            return item.Key;
          });

        $scope.model.ChecklistItemIds = $scope.checklist.Categories
          .flatMap(c => c.Items)
          .filter(i => i.IsChecked)
          .map(i => i.Id);

        let bsdoc = null;
        if ($scope.model.BusinessCaseDocument) {
          bsdoc = $scope.model.BusinessCaseDocument.File;
        }

        return {
          summary: angular.toJson($scope.model),
          file: bsdoc
        };
      },
      backup: true
    },
    loaded: function () {
      return $scope.model && $scope.model.RowVersion && $scope.grantFile && $scope.model.RowVersion === $scope.grantFile.RowVersion;
    },
    onSave: function () {
      $scope.resyncApplicationDetails();
      $scope.emit('refresh', { force: true });
    },
    onRefresh: function () {
      return loadSummary().catch(angular.noop);
    },
    maxTrainingPeriodDate: null
  };

  if (typeof ($scope.primaryAssessors) === 'undefined') $scope.primaryAssessors = [];
  if (typeof ($scope.assessors) === 'undefined') $scope.assessors = [];
  if (typeof ($scope.riskClassifications) === 'undefined') $scope.riskClassifications = [];
  if (typeof ($scope.programInitiatives) === 'undefined') $scope.programInitiatives = [];
  if (typeof ($scope.deliveryPartners) === 'undefined') $scope.deliveryPartners = [];
  if (typeof ($scope.deliveryPartnerServices) === 'undefined') $scope.deliveryPartnerServices = [];
  if (typeof ($scope.checklist) === 'undefined') $scope.checklist = [];

  angular.extend(this, $controller('Section', { $scope: $scope, $attrs: $attrs }));

  /**
   * Make AJAX request for primary assessors data
   * @function loadPrimaryAssessors
   * @returns {Promise}
   **/
  function loadPrimaryAssessors() {
    return $scope.load({
      url: '/Int/Application/PrimaryAssessors/' + $scope.parent.grantApplicationId,
      set: 'primaryAssessors',
      condition: !$scope.primaryAssessors || !$scope.primaryAssessors.length
    });
  }

  /**
   * Make AJAX request for assessors data
   * @function loadAssessors
   * @returns {Promise}
   **/
  function loadAssessors() {
    return $scope.load({
      url: '/Int/Application/Assessors/' + $scope.parent.grantApplicationId,
      set: 'assessors',
      condition: !$scope.assessors || !$scope.assessors.length
    });
  }

  /**
   * Make AJAX request for risk classifications
   * @function loadRiskClassifications
   * @returns {Promise}
   **/
  function loadRiskClassifications() {
    return $scope.load({
      url: '/Int/Application/Summary/Risk/Classifications',
      set: 'riskClassifications',
      condition: !$scope.riskClassifications || !$scope.riskClassifications.length,
      localCache: true
    });
  }

  function loadProgramInitiatives() {
    return $scope.load({
      url: '/Int/Application/Summary/ProgramInitiatives',
      set: 'programInitiatives',
      condition: !$scope.programInitiatives || !$scope.programInitiatives.length,
      localCache: false
    });
  }

  /**
   * Make AJAX request for delivery partners
   * @function loadDeliveryPartners
   * @returns {Promise}
   **/
  function loadDeliveryPartners() {
    return $scope.load({
      url: '/Int/Application/Summary/Delivery/Partners/' + $scope.grantFile.GrantProgramId,
      set: 'deliveryPartners',
      condition: !$scope.deliveryPartners || !$scope.deliveryPartners.length,
      localCache: false
    });
  }

  /**
   * Make AJAX request for delivery partner services
   * @function loadDeliveryPartnerServices
   * @returns {void}
   **/
  function loadDeliveryPartnerServices() {
    return $scope.load({
      url: '/Int/Application/Summary/Delivery/Partner/Services/' + $scope.grantFile.GrantProgramId,
      set: 'deliveryPartnerServices',
      condition: !$scope.deliveryPartnerServices || !$scope.deliveryPartnerServices.length,
      localCache: false
    });
  }

  /**
   * Make AJAX request for checklist data
   * @function loadSummary
   * @returns {void}
   **/
  function loadChecklist() {
    return $scope.load({
      url: '/Int/Application/Checklist/' + $scope.parent.grantApplicationId,
      set: 'checklist',
      localCache: false
    });
  }

  /**
   * Make AJAX request for summary data
   * @function loadSummary
   * @returns {void}
   **/
  function loadEligibilityInformation() {
    return $scope.load({
      url: '/Int/Application/Eligibility/' + $scope.parent.grantApplicationId,
      set: 'eligibility',
      localCache: false
    });
  }

  /**
   * Make AJAX request for summary data
   * @function loadSummary
   * @returns {void}
   **/
  function loadSummary() {
    return $scope.load({
      url: '/Int/Application/Summary/' + $scope.parent.grantApplicationId,
      set: 'model'
    })
      .then(function () {
        $scope.section.maxTrainingPeriodDate = getTrainingPeriodMaxDate(); 
      });
  }

  /**
   * Initialize the data for the form
   * @function init
   * @returns {void{
   **/
  $scope.init = function() {
    return Promise.all([
      loadPrimaryAssessors(),
      loadAssessors(),
      loadRiskClassifications(),
      loadProgramInitiatives(),
      loadDeliveryPartners(),
      loadDeliveryPartnerServices(),
      loadChecklist(),
      loadEligibilityInformation()
    ]).then(function() {
      loadSummary();
    }).catch(angular.noop);
  }

  /**
   * Reassign the primary assessor for this application.
   * @function reassign
   * @returns {Promise}
   **/
  $scope.reassignPrimary = function () {
    return $scope.load({
        url: '/Int/Application/Summary/AssignPrimary',
        data: function() {
          return $scope.model;
        },
        set: 'model',
        method: 'PUT'
      })
      .then(function() {
        return $scope.section.onSave();
      })
      .catch(angular.noop);
  }
  
  /**
   * Reassign the assessor for this application.
   * @function reassign
   * @returns {Promise}
   **/
  $scope.reassign = function () {
    return $scope.load({
        url: '/Int/Application/Summary/Assign',
        data: function() {
          return $scope.model;
        },
        set: 'model',
        method: 'PUT'
      })
      .then(function() {
        return $scope.section.onSave();
      })
      .catch(angular.noop);
  }

  /**
   * Get the maximum training period date.
   * @function getTrainingPeriodMaxDate
   * @return {Date}
   **/
  function getTrainingPeriodMaxDate() {
    if (Utils.isDate($scope.model.TrainingPeriodStartDate)) {
      return new Date($scope.model.TrainingPeriodStartDate.getFullYear() + 1, $scope.model.TrainingPeriodStartDate.getMonth(), $scope.model.TrainingPeriodStartDate.getDay());
    }
    return;
  }


  /**
   * Open the modal file uploaded.
   * @function openAttachmentModal
   * @param {any} title - The title of the modal window.
   * @param {any} attachment - The attachment to update/add.
   * @returns {Promise}
   */
  function openAttachmentModal(title, attachment) {
    return ngDialog.openConfirm({
      template: '/content/dialogs/_TrainingProviderAttachment.html',
      data: {
        title: title,
        attachment: attachment
      },
      controller: function ($scope) {
        /**
         * Return the selected file in the promise.
         * @function save
         * @returns {Promise}
         **/
        $scope.save = function () {
          $scope.confirm($scope.ngDialogData.attachment);
        };

        /**
         * Manually call the file select.
         * @function chooseFile
         * @returns {void}
         **/
        $scope.chooseFile = function () {
          var $input = angular.element('#training-provider-upload');
          $input.trigger('click');
        }

        /**
         * Set the selected file as the active attachment.
         * @function fileChanged
         * @param {any} $files
         * @returns {void}
         */
        $scope.fileChanged = function ($files) {
          if ($files.length) {
            $scope.ngDialogData.attachment.File = $files[0];
            $scope.ngDialogData.attachment.FileName = $scope.ngDialogData.attachment.File.name;
          }
        }
      }
    });
  }

  /**
   * Open modal file uploader popup and then add the new file to the model.
   * @function addAttachment
   * @param {string} prop - The name of the property for this attachment.
   * @returns {void}
   **/
  $scope.addAttachment = function (prop) {
    openAttachmentModal('Add Attachment', {
      Id: 0,
      FileName: '',
      Description: '',
      File: {}
    })
      .then(function (attachment) {
        $scope.model[prop] = attachment;
      })
      .catch(angular.noop);
  }

  /**
   * Open modal file uploader popup and allow user to updte the attachment and/or file.
   * @function changeAttachment
   * @param {string} prop - The name of the property for this attachment.
   * @returns {void}
   */
  $scope.changeAttachment = function (prop) {
    openAttachmentModal('Change Attachment', $scope.model[prop])
      .then(function (attachment) {
        prop = attachment;
      })
      .catch(angular.noop);
  }

  /**
   * Download the specified attachment.
   * @function downloadAttachment
   * @param {any} attachmentId - The attachment id.
   * @returns {void}
   */
  $scope.downloadAttachment = function (attachmentId) {
    window.open('/Int/Application/Attachment/Download/' + $scope.model.Id + '/' + attachmentId);
  }
});
