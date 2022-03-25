import axios from "axios";
import cookie from "vue-cookies";
import router from "../../router/index";

const state = {
  events: [],
  validCourse: true,
  Name: ""
};

const getters = {
  getCourseEvents: state => state.events,
  validCourse: state => state.validCourse,
  getCourseName: state => state.Name
};

const actions = {
  async fetchCourseEvents({ commit }, id) {
    const token = cookie.get("USOSFIX_TOKEN");
    if (token != null && token != undefined) {
      await axios.get(
        `Timetable/SubjectGroups?token=${token}&subjectId=${id}`
      ).then((response) => {
        var events = [];
        response.forEach(event => {
          events.push({
            start: createDate2(event.meetings[0].startTime),
            end: createDate2(event.meetings[0].endTime),
            class: event.classType.toLowerCase(),
            classType: event.classType.toLowerCase(),
            title: {
              "pl": "Grupa " + event.groupNumber,
              "en": "Group " + event.groupNumber,
            },
            content: {
              "pl": `${event.meetings[0].building.pl} ${event.meetings[0].room}`,
              "en": `${event.meetings[0].building.en} ${event.meetings[0].room}`,
            },
            id: event.id,
            lecturer: event.lecturers,
          });
        });
        commit("setName", response[0].subject.name);
        commit("setCourseEvents", events);
      }).catch((handled) => {
        if(!handled) {
          commit("setNotValid");
        }
      });
    } else {
      commit("logOut");
      router.push({ name: "Login", params: { msg: "Session expired", type: "expired" } });
    }
  },
  async clearCourseData({commit}) {
    commit("setNoCourse");
  }
};


const mutations = {
  setCourseEvents: (state, events) => {
    state.events = events;
    state.validCourse = true;
  },
  setNoCourse: (state) => {
    state.events = [];
  },
  setNotValid: (state) => {
    state.events = [];
    state.validCourse = false;
  },
  setName: (state, name) => state.Name = name,
};

export default {
  state,
  getters,
  actions,
  mutations
};

function createDate2(string) {
  let date = new Date(string);
  return new Date((new Date()).getFullYear(), (new Date()).getMonth(), (new Date()).getDate() - (new Date()).getDay() - ((new Date()).getDay() == 0? 7: 0) + date.getDay(), date.getHours(), date.getMinutes());
}