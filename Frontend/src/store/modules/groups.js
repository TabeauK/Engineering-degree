import axios from "axios";
import cookie from "vue-cookies";
import router from "../../router/index";

const state = {
  groups: [],
  loadingTeams: true,
  suggestionsForTeam: {},
};

const getters = {
  getGroups: state => {
    state.groups.sort((x, y) => y.students.length - x.students.length);
    return state.groups;
  },
  getLoadingTeams: state => state.loadingTeams,
  getSuggestionsForTeam: state =>  state.suggestionsForTeam,
};

const actions = {
  async fetchGroups({commit}) {
    commit("setLoadingTeams", true);
    const token = cookie.get("USOSFIX_TOKEN");
    if(token != null && token != undefined) {
      await axios.get(
        `Teams/MyTeams?token=${token}`
      ).then((response) => {
        var groups = [];
        response.partOf.forEach(group => {
          let g = {
            id: group.id,
            name: group.subject.name,
            subjectId: group.subject.id,
            students: [],
          };
          group.users.forEach(student => {
            g.students.push({
              id: student.id,
              name: student.displayName + "#" + student.id.toLocaleString("en-US", {
                minimumIntegerDigits: 4,
                useGrouping: false
              }),
              status: "Accepted",
              inviteId: 0,
            });
          });
          group.invitations.forEach(invite => {
            g.students.push({
              id: invite.invited.id,
              name: invite.invited.displayName + "#" + invite.invited.id.toLocaleString("en-US", {
                minimumIntegerDigits: 4,
                useGrouping: false
              }),
              inviting: invite.inviting.displayName + "#" + invite.inviting.id.toLocaleString("en-US", {
                minimumIntegerDigits: 4,
                useGrouping: false
              }),
              status: "Pending",
              inviteId: invite.id,
            });
          });
          groups.push(g);
        });
        response.invitedTo.forEach(group => {
          let g = {
            id: group.id,
            name: group.subject.name,
            subjectId: group.subject.id,
            students: [],
          };
          group.users.forEach(student => {
            g.students.push({
              id: student.id,
              name: student.displayName + "#" + student.id.toLocaleString("en-US", {
                minimumIntegerDigits: 4,
                useGrouping: false
              }),
              status: "Accepted",
              inviteId: 0,
            });
          });
          group.invitations.forEach(invite => {
            g.students.push({
              id: invite.invited.id,
              name: invite.invited.displayName + "#" + invite.invited.id.toLocaleString("en-US", {
                minimumIntegerDigits: 4,
                useGrouping: false
              }),
              inviting: invite.inviting.displayName + "#" + invite.inviting.id.toLocaleString("en-US", {
                minimumIntegerDigits: 4,
                useGrouping: false
              }),
              invitingId: invite.inviting.id,
              status: "Pending",
              inviteId: invite.id,
            });
          });
          groups.push(g);
        });
        commit("setGroups", groups);
      }).catch((handled) => {
        if(!handled) {
          commit("setErrorMsg", "There was an error while getting your teams. Try again later...");
        }
      });
    } else {
      commit("logOut");
      router.push({ name: "Login", params: { msg: "Session expired", type: "expired" } });
    }
  },

  async acceptTeamInvite({commit}, id) {
    const token = cookie.get("USOSFIX_TOKEN");
    if (token != null && token != undefined) {
      await axios.put(
        `Teams/AcceptInvitation?token=${token}&invitationId=${id}`
      ).then(() => actions.fetchGroups({commit})).catch((handled) => {
        if(!handled) {
          commit("setErrorMsg", "There was an error while accepting invite. Try again later...");
        }
      });
    } else {
      router.push({ name: "Login", params: { msg: "Session expired", type: "expired" } });
    }
  },
  async rejectTeamInvite({commit}, id) {
    const token = cookie.get("USOSFIX_TOKEN");
    if (token != null && token != undefined) {
      await axios.put(
        `Teams/DeleteInvitation?token=${token}&invitationId=${id}`
      ).then(() => actions.fetchGroups({commit})).catch((handled) => {
        if(!handled) {
          commit("setErrorMsg", "There was an error while rejecting invite. Try again later...");
        }
      });
    } else {
      router.push({ name: "Login", params: { msg: "Session expired", type: "expired" } });
    }
  },
  async removeUser({commit}, props) {
    const token = cookie.get("USOSFIX_TOKEN");
    if (token != null && token != undefined) {
      await axios.put(
        `Teams/DeleteUserFromTeam?token=${token}&teamId=${props.id}&userId=${props.user}`
      ).then(() => {
        commit("removeUser", props);
        actions.fetchGroups({commit});
      }).catch((handled) => {
        if(!handled) {
          commit("setErrorMsg", "There was an error while removing user from team. Try again later...");
        }
      });
    } else {
      commit("logOut");
      router.push({ name: "Login", params: { msg: "Session expired", type: "expired" } });
    }
  },
  async stopInvitingFromGroup({commit}, id) {
    const token = cookie.get("USOSFIX_TOKEN");
    if (token != null && token != undefined) {
      await axios.put(
        `Teams/DeleteInvitation?token=${token}&invitationId=${id}`
      ).then(() => {
        actions.fetchGroups({commit});
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
  async fetchSuggestionsForTeam({commit}, data) {
    const token = cookie.get("USOSFIX_TOKEN");
    const subjectId = state.groups.find(x => x.id == data.teamId).subjectId;
    if (token != null && token != undefined) {
      await axios.get(
        `Teams/SubjectTeamlessUserSearch?token=${token}&prefix=${data.prefix}&subjectId=${subjectId}`
      ).then((suggestions) => commit("updateSuggestionsForTeam", {suggestions, teamId: data.teamId})).catch((handled) => {
        if(!handled) {
          commit("setErrorMsg", "There was an error while getting users. Try again later...");
        }
      });
    } else {
      router.push({ name: "Login", params: { msg: "Session expired", type: "expired" } });
    }
  },
  async inviteUserToTeam({commit}, data) {
    const token = cookie.get("USOSFIX_TOKEN");
    const subjectId = state.groups.find(x => x.id == data.teamId).subjectId;
    if (token != null && token != undefined) {
      await axios.put(
        `Teams/InviteUser?token=${token}&subjectId=${subjectId}&invitedId=${data.id}`
      ).then(async () => {
        actions.fetchGroups({commit});
      }).catch((handled) => {
        if(!handled) {
          commit("setErrorMsg", "There was an error while inviting user. Try again later...");
        }
      });
    } else {
      router.push({ name: "Login", params: { msg: "Session expired", type: "expired" } });
    }
  }
};

const mutations = {
  setGroups: (state, groups) => state.groups = groups,
  removeUser: (state, props) => {
    var s = state.groups.find(group => group.id == props.id).students;
    var index = s.findIndex(student => student.id == props.user);
    state.groups.find(group => group.id == props.id).students.splice(index,1);
    if(state.groups.find(group => group.id == props.id).students < 1) {
      state.groups.splice(state.groups.findIndex(group => group.id == props.id), 1);
    }
  },
  setLoadingTeams: (state, value) => state.loadingTeams = value,
  updateSuggestionsForTeam: (state, data) => {
    var students = state.groups.find(x => x.id == data.teamId).students;
    var brr = data.suggestions.filter(x => !students.some(y => y.id == x.id));
    var arr = [];
    brr.forEach(x => {
      arr.push({name: x.displayName + "#" + x.id.toLocaleString("en-US", {
        minimumIntegerDigits: 4,
        useGrouping: false
      }), value: x.id});
    });
    state.suggestionsForTeam[data.teamId] = arr;
  }
};

export default {
  state,
  getters,
  actions,
  mutations
};
