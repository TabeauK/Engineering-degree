import axios from "axios";
import cookie from "vue-cookies";
import router from "../../router/index";

const state = {
  events: [],
  usosEvents: [],
  showWeekends: true,
  showEvents: true,
  showUsosEvents: false,
  subjects: [],
};

const getters = {
  getEvents: state => state.showEvents ? state.events : [],
  getUsosEvents: state => state.showUsosEvents ? state.usosEvents : [],
  getShowWeekends: state => state.showWeekends,
  getShowEvents: state => state.showEvents,
  getShowUsosEvents: state => state.showUsosEvents,
  getSubjects: state => state.subjects,
};

const actions = {
  async fetchEvents({commit}) {
    const token = cookie.get("USOSFIX_TOKEN");
    commit("setLoadingSubjects", true);
    if(token != null && token != undefined) {
      await axios.get(
        `Timetable/UserGroups?token=${token}`
      ).then((response) => {
        var events = [];
        response.forEach(event => {
          events.push({
            start: createDate(event.meetings[0].startTime),
            end: createDate(event.meetings[0].endTime),
            class: event.classType.toLowerCase(),
            classType: event.classType.toLowerCase(),
            title: event.subject.name,
            content: {
              "pl": event.meetings[0].building.pl + " " + event.meetings[0].room,
              "en": event.meetings[0].building.en + " " + event.meetings[0].room,
            },
            id: event.subject.id,
            lecturer: event.lecturers,
          });
        });
        commit("setUsosEvents", events);
      }).catch((handled) => {
        if(!handled) {
          commit("setErrorMsg", "There was an error while getting plan. Try again later...");
        }
      });
      await axios.get(
        `Timetable/UserGroupsAfterExchanges?token=${token}`
      ).then(async (response) => {
        var events = [];
        response.forEach(event => {
          events.push({
            start: createDate(event.meetings[0].startTime),
            end: createDate(event.meetings[0].endTime),
            class: event.classType.toLowerCase() + " " + event.state,
            classType: event.classType.toLowerCase(),
            title: event.subject.name,
            content: {
              "pl": event.meetings[0].building.pl + " " + event.meetings[0].room,
              "en": event.meetings[0].building.en + " " + event.meetings[0].room,
            },
            id: event.subject.id,
            lecturer: event.lecturers,
          });
        });
        var distinctEvents = [];
        events.forEach((subject) => {
          if(!distinctEvents.some(x => x.id == subject.id)) {
            distinctEvents.push({id: subject.id, name: subject.title});
          }
        });
        var subjects = [];
        let promises = [];
        distinctEvents.forEach((subject) => {
          promises.push(
            axios.get(
              `Exchanges/ExchangesSummaryBySubject?token=${token}&subjectId=${subject.id}`
            ).then((summary) => {
              subjects.push({
                id: subject.id,
                name: subject.name,
                submitted: summary.submitted,
                accepted: summary.accepted,
                rejected: summary.rejected,
              });
            }));
        });
        Promise.all(promises).then(() => {
          commit("setSubjects", subjects);
          commit("setEvents", events);
          commit("setLoadingSubjects", false);
        });
      }).catch((handled) => {
        if(!handled) {
          commit("setErrorMsg", "There was an error while getting plan. Try again later...");
        }
      });
    } else {
      commit("logOut");
      router.push({ name: "Login", params: { msg: "Session expired", type: "expired" } });
    }
  },
  toggleShowWeekends({commit}, value) {
    commit("toggleShowWeekends", value);
  },
  toggleShowEvents({commit}, value) {
    commit("toggleShowEvents", value);
  },
  toggleShowUsosEvents({commit}, value) {
    commit("toggleShowUsosEvents", value);
  },
  clearEvents({commit}) {
    commit("clearEvents");
  }
};

const mutations = {
  setEvents: (state, events) => {
    state.events = events;
  },
  setUsosEvents: (state, usosEvents) => {
    state.usosEvents = usosEvents;
  },
  toggleShowWeekends: (state, value) => {
    state.showWeekends = value;
  },
  toggleShowEvents: (state, value) => {
    state.showEvents = value;
  },
  toggleShowUsosEvents: (state, value) => {
    state.showUsosEvents = value;
  },
  clearEvents: (state) => {
    state.events = [];
    state.UsosEvents = [];
  },
  setSubjects: (state, subjects) => state.subjects = subjects,
};

export default {
  state,
  getters,
  actions,
  mutations
};

function createDate(string) {
  let date = new Date(string);
  return new Date((new Date()).getFullYear(), (new Date()).getMonth(), (new Date()).getDate() - (new Date()).getDay() - ((new Date()).getDay() == 0? 7: 0) + date.getDay(), date.getHours(), date.getMinutes());
}