import { createStore } from "vuex";
import auth from "./modules/auth";
import events from "./modules/events";
import course from "./modules/course";
import courseGroup from "./modules/courseGroup";
import groups from "./modules/groups";
import exchanges from "./modules/exchanges";
import settings from "./modules/settings";
import messages from "./modules/messages";
import error from "./modules/error";
import language from "./modules/language";

export default createStore({
  modules: {
    auth,
    events,
    course,
    courseGroup,
    groups,
    exchanges,
    settings,
    messages,
    error,
    language
  }
});
