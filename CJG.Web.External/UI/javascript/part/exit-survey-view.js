var participantSessionIdleTime = 0;
var participantSessionNotification = 5;
var participantSessionWarning = false;

app.controller('ExitSurveyView', function($scope, $controller, $attrs, $timeout, $sce) {
  $scope.section = {
    sessionDuration: $attrs.ngSessionDuration || 0,
    validRecaptcha: false,
    invitationKey: $attrs.ngInvitationKey,
    loadedParticipantFormId: null,

    save: {
      url: '/Part/ExitForm/Submit',
      method: 'POST',
      data: function() {
        return $scope.model;
      },
      set: 'model'
    },
    onSave: function (event, data) {
      if (data.response.data.RedirectURL) 
        window.location = data.response.data.RedirectURL;
    }
  };

  angular.extend(this, $controller('Section', { $scope: $scope, $attrs: $attrs }));

  function initializeTimer() {
    if ($scope.section.sessionDuration) {
      //Increment the idle time counter every minute.
      setInterval(timerIncrement, 60000);

      //Zero the idle timer on mouse movement.
      angular.element(this).mousedown(function(e) {
        participantSessionReset();
      });
      angular.element(this).keypress(function(e) {
        participantSessionReset();
      });

      if ($scope.section.sessionDuration <= 5)
        participantSessionNotification = 1;

      timerIncrement();
    }
  }

  function timerIncrement() {
    if (++participantSessionIdleTime > $scope.section.sessionDuration) {
      participantSessionTimeout();
    } else if (participantSessionIdleTime == $scope.section.sessionDuration - participantSessionNotification + 1) {
      sessionTimeout();
    }
  }

  function sessionTimeout() {
    var message = "Your session will be timeout in " +
      participantSessionNotification +
      " minute" +
      (participantSessionNotification > 1 ? "s" : "") +
      ", and your data will be lost. Please click [OK] if you would like to stay on the page.";

    participantSessionWarning = true;

    return $scope.messageDialog('Session Timeout', message)
      .then(function() {
        participantSessionWarning = false;
        participantSessionReset();
      })
      .catch(angular.noop);
  }

  function participantSessionReset() {
    if (!participantSessionWarning) {
      participantSessionIdleTime = 0;
    }
  }

  function participantSessionTimeout() {
    window.location = '/Part/ExitForm/Timeout';
  }

  initializeTimer();

  /**
   * Setup and initialize the Recaptcha form.
   * @function setupRecaptcha
   * @returns {void}
   **/
  function setupRecaptcha() {
    var el = document.getElementById('recaptcha');
    var key = el && el.getAttribute('data-site-key');
    if (el && key) {
      try {
        grecaptcha.render(el,
          {
            'sitekey': key,
            'callback': function(token) {
              return $timeout(function () {
                $scope.model.RecaptchaEncodedResponse = token;
                $scope.section.validRecaptcha = true;
              });
            },
            'expired-callback': function(token) {
              return $timeout(function() {
                $scope.model.RecaptchaEncodedResponse = null;
                $scope.section.validRecaptcha = false;
              });
            }
          });
      } catch (ex) {
        return $scope.messageDialog('ReCaptcha',
            '<p>Google Recaptcha failed to load, please try again later or contact support.</p><p>' +
            ex.message +
            '</p>')
          .catch(angular.noop);
      }
    } else {
      return $scope.messageDialog('ReCaptcha',
          'Google Recaptcha failed to load, please try again later or contact support.')
        .catch(angular.noop);
    }
  }

  window.onRecaptchaLoad = function() {
    if (angular.element('#exit-survey-info').length) {
      setupRecaptcha();
    }
    angular.element('#ng-loading-overlay').addClass('ng-hide');
  }

  var location = window.location.href.toLowerCase();
  if (location.indexOf("/part/") > -1) {
    // need to remove the menu if logged in
    // find the url and set to lower case
    // hide the menus
    angular.element('.desktop-menu, .mobile-menu').hide();
  }

  function loadExitSurvey() {
    return $scope.load({
        url: '/Part/ExitForm/Data/' + ($scope.section.invitationKey),
        set: 'model'
      })
      .catch(angular.noop);
  }
  
  $scope.getParticipantForm = function () {
    return $scope.ajax({
        url: '/Part/ExitForm/FindPIF/',
        method: 'POST',
        data: function() {
          return {
            invitationKey: $scope.model.InvitationKey,
            firstName: $scope.model.FirstName,
            lastName: $scope.model.LastName,
            dateOfBirth: $scope.model.DateOfBirth
          }
        }
      })
      .then(function(response) {
        let pifId = response.data;
        $timeout(function() {
          $scope.model.ParticipantFormId = pifId;
        });
      });
  }

  $scope.updateSelection = function (position, entities) {
    angular.forEach(entities, function (subscription, index) {
      if (position != index)
        subscription.AnswerGiven = false;
    });
  }

  function init() {
    return Promise.all([
        loadExitSurvey()
      ])
      .catch(angular.noop);
  }

  init();
});
