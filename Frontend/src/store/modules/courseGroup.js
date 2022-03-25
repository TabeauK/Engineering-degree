import axios from "axios";
import cookie from "vue-cookies";
import router from "../../router/index";

const state = {
  groupId: 0,
  validGroup: true,
  courseId: 0,
  studnetStatus: "",
  data: {
    courseName: "",
    name: "",
    type: "",
    lecturer: "",
    dates: [],
    place: "",
    placeLimit: 0,
    irregular: false,
    irregularity: "",
  },
  students: [],
  exchange: {},
  loadingStudents: true,
};

const getters = {
  getCourseId: state => state.courseId,
  validGroup: state => state.validGroup,
  getGroupData: state => state.data,
  getStudents: state => {
    let students = Array();
    for(let i=0;i< state.students.length; i++) {
      students.push({nr: i+1,
        userId: state.students[i].id, 
        name: state.students[i].name,
        status: state.students[i].status,
        inviteId: state.students[i].invite});
    }
    return students;
  },
  getStudnetStatus: state => state.studentStatus, 
  getLoadingstudents: state => state.loadingStudents, 
};

const actions = {
  async fetchCourseGroup({commit}, id) {
    commit("setLoadingstudents", true);
    commit("setGroupId", id);
    const token = cookie.get("USOSFIX_TOKEN");
    if (token != null && token != undefined) {
      await axios.get(
        `Timetable/GroupInfo?token=${token}&groupId=${id}`
      ).then(async (response) => {
        await axios.get(
          `Timetable/UserGroups?token=${token}`
        ).then(async (groups) => {
          await axios.get(
            `Exchanges/Exchanges?token=${token}`
          ).then(async (exchanges) => {
            await axios.get(
              `Teams/MyTeams?token=${token}`
            ).then((teams) => {
              commit("setValidGroup", true);
              commit("setCourseId", response.subject.id);
              if(exchanges.length > 0 && exchanges.some(x => x.groupTo.id == id && x.exchangeState === "Submitted")) {
                commit("setStudentStatus", "AlreadyJoining");
                commit("setExchange", exchanges.find(x => x.groupTo.id == id));
              } else if (groups.length > 0 && (groups).some(x => x.id == id)) {
                commit("setStudentStatus", "AlreadyIn");
              } else {
                commit("setStudentStatus", "CanJoin");
              }
              let dates = [];
              let meetings = [];
              response.meetings.forEach(meet => meetings.push({start: new Date(meet.startTime), end: new Date(meet.endTime)}));
              meetings.sort((x,y) => x.start-y.start);
              meetings.forEach(meet => {
                dates.push(format(meet.start) + " - " + format(meet.end).substring(11));
              });
              commit("setData", {
                courseName: response.subject.name,
                name: {
                  "pl": "Grupa " + response.groupNumber,
                  "en": "Group " + response.groupNumber,
                },
                type: response.classType.toLowerCase(),
                lecturer: response.lecturers,
                dates: dates,
                place: {
                  "pl": response.meetings[0].building.pl + " " + response.meetings[0].room,
                  "en": response.meetings[0].building.en + " " + response.meetings[0].room,
                },
                placeLimit: response.maxMembers,
                //irregular: true,
                //irregularity: "W parzyste tygodnie",
              });
              let invites = [];
              if(teams.partOf.some(x => x.subject.id  == response.subject.id)) {
                invites = teams.partOf.find(x => x.subject.id == response.subject.id).invitations;
              }
              let students = [];
              response.students.forEach(student => {
                let status = "";
                let inviteId = "";
                if(invites.some(x => x.invited.id == student.id)) {
                  inviteId = invites.find(x => x.invited.id == student.id).id;
                  status = "invited";
                }
                students.push({
                  id: student.id,
                  name: student.displayName + "#" + student.id.toLocaleString("en-US", {
                    minimumIntegerDigits: 4,
                    useGrouping: false
                  }),
                  status: status,
                  invite: inviteId,
                });
              });
              commit("setStudents", students);
            }).catch((handled) => {
              if(!handled) {
                commit("setNoGroup");
              }
            });
          }).catch((handled) => {
            if(!handled) {
              commit("setNoGroup");
            }
          });
        }).catch((handled) => {
          if(!handled) {
            commit("setNoGroup");
          }
        });
      }).catch((handled) => {
        if(!handled) {
          commit("setNoGroup");
        }
      });
      commit("setLoadingstudents", false);
    } else {
      commit("logOut");
      router.push({ name: "Login", params: { msg: "Session expired", type: "expired" } });
    }
  },
  async join({commit}) {
    const token = cookie.get("USOSFIX_TOKEN");
    if (token != null && token != undefined) {
      await axios.put(
        `Exchanges/AddExchange?token=${token}&groupToId=${state.groupId}`
      ).then(async () => {
        await axios.get(
          `Exchanges/Exchanges?token=${token}`
        ).then(async (exchanges) => {
          if(exchanges.length > 0 && exchanges.some(x => x.groupTo.id == state.groupId)) {
            commit("setStudentStatus", "AlreadyJoining");
            commit("setExchange", exchanges.find(x => x.groupTo.id == state.groupId));
          } else {
            commit("setStudentStatus", "CanJoin");
          }
        }).catch((handled) => {
          if(!handled) {
            commit("setNoGroup");
          }
        });
      }).catch((handled) => {
        if(!handled) {
          commit("setErrorMsg", "There was an error while submiting join. Try again later...");
        }
      });
    } else {
      commit("logOut");
      router.push({ name: "Login", params: { msg: "Session expired", type: "expired" } });
    }
  },
  async stopJoining({commit}) {
    const token = cookie.get("USOSFIX_TOKEN");
    if (token != null && token != undefined) {
      await axios.delete(
        `Exchanges/DeleteExchange?token=${token}&exchangeId=${state.exchange.id}`
      ).then(() => {
        commit("setStudentStatus", "CanJoin");
        commit("setExchange", {});
      }).catch((handled) => {
        if(!handled) {
          commit("setErrorMsg", "There was an error while removing exchange. Try again later...");
        }
      });
    } else {
      commit("logOut");
      router.push({ name: "Login", params: { msg: "Session expired", type: "expired" } });
    }
  },
  async invite({commit}, userId) {
    const token = cookie.get("USOSFIX_TOKEN");
    if (token != null && token != undefined) {
      await axios.put(
        `Teams/InviteUser?token=${token}&subjectId=${state.courseId}&invitedId=${userId}`
      ).then(async () => {
        actions.fetchCourseGroup({commit}, state.groupId);
      }).catch((handled) => {
        if(!handled) {
          commit("setErrorMsg", "There was an error while inviting user. Try again later...");
        }
      });
    } else {
      commit("logOut");
      router.push({ name: "Login", params: { msg: "Session expired", type: "expired" } });
    }
  },
  async stopInviting({commit}, props) {
    const token = cookie.get("USOSFIX_TOKEN");
    if (token != null && token != undefined) {
      await axios.put(
        `Teams/DeleteInvitation?token=${token}&invitationId=${props.inviteId}`
      ).then(async () => {
        actions.fetchCourseGroup({commit}, state.groupId);
      }).catch((handled) => {
        if(!handled) {
          commit("setErrorMsg", "There was an error while removing invite. Try again later...");
        }
      });
    } else {
      commit("logOut");
      router.push({ name: "Login", params: { msg: "Session expired", type: "expired" } });
    }
  },
};

const mutations = {
  setNoGroup: (state) => {
    state.validGroup = false;
    state.courseId = -1;
    state.students = Array();
    state.data = {};
    state.studnetStatus = "";
  },
  setGroupId: (state, id) => state.groupId = id,
  setValidGroup: (state, valid) => state.validGroup = valid,
  setCourseId: (state, id) => state.courseId = id,
  setStudentStatus: (state, status) => state.studentStatus = status,
  setData: (state, data) => state.data = data,
  setStudents: (state, students) => state.students = students,
  setExchange: (state, exchange) => state.exchange = exchange,
  setLoadingstudents: (state, value) => state.loadingStudents = value, 
};

export default {
  state,
  getters,
  actions,
  mutations
};

const format = (d) => ("0" + d.getDate()).slice(-2) + "-" + ("0"+(d.getMonth()+1)).slice(-2) + "-" + d.getFullYear() + " " + ("0" + d.getHours()).slice(-2) + ":" + ("0" + d.getMinutes()).slice(-2);