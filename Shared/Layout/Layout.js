/* Tabs */
if (Model.AskALawyerTabActive)
    document.getElementById("_askALawyerTab").className = "active";
if (Model.BrowseAnswersTabActive)
    document.getElementById("_browseAnswersTab").className = "active";
if (Model.HowItWorksTabActive)
    document.getElementById("_howItWorksTab").className = "active";

/* Login bar */
if (Model.User !== null) {
    document.getElementById("_notLoggedIn").style.display = "none";
    document.getElementById("_loggedIn").style.display = "block";
}