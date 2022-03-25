import axios from "axios";
import cookie from "vue-cookies";
import router from "../../router/index";

const state = {
  name: "",
  surname: "",
  username: "",
  usernameHash: "",
  id: 0,
  role: "",
  leaders: [],
  admins: [],
  index: 0,
  revealed: false,
  loadingLeaders: true,
  loadingAdmins: true,
  loadingSubjects: true,
};

const getters = {
  getName: state => state.name,
  getSurname: state => state.surname,
  getUsername: state => state.username,
  getId: state => state.id,
  getRole: state => state.role,
  getLeaders: state => state.leaders,
  getAdmins: state => state.admins,
  getIndex: state => state.index,
  getRevealed: state => state.revealed,
  getLoadingLeaders: state => state.loadingLeaders,
  getLoadingAdmins: state => state.loadingAdmins,
  getLoadingSubjects: state => state.loadingSubjects,
};

const actions = {
  async fetchUserInfo({commit}) {
    //cookie.set("USOSFIX_TOKEN", "naCbbVhM32P7YWDThsRe", { expires: "2h" }); //dev
    commit("setLoadingLeaders", true);
    commit("setLoadingAdmins", true);
    const token = cookie.get("USOSFIX_TOKEN");  
    if(token != null && token != undefined) {
      await axios.get(
        `Account/WhoAmI?token=${token}`
      ).then(async (data) => {
        cookie.set("USOSFIX_TOKEN", token, { expires: "2h" });
        commit("setName", data.name);
        commit("setSurname", data.surname);
        commit("setIndex", data.studentNumber);
        commit("setUsername", data.username);
        commit("setId", data.id);
        commit("setLanguage", data.preferredLanguage == "English"? "en" : "pl");
        commit("setRole", data.role);
        commit("setRevealed", data.visible);
        if(data.role == "Admin") {
          await axios.get(
            `Roles/AdminsAndLeaders?token=${token}`
          ).then((data2) => {
            commit("setAdmins", data2.filter(x => x.role == "Admin"));
            commit("setLeaders", data2.filter(x => x.role == "Leader"));
          }).catch((handled) => {
            if(!handled) {
              commit("setErrorMsg", "There was an error while getting admins and leaders. Try again later...");
            }
          });
        }
      }).catch((handled) => {
        if(!handled) {
          commit("setErrorMsg", "There was an error while getting your account's data. Try again later...");
        }
      });
      commit("setLoadingLeaders", false);
      commit("setLoadingAdmins", false);
    } else {
      commit("logOut");
      router.push({ name: "Login", params: { msg: "Session expired", type: "expired" }});
    }
  },
  async saveUsername({commit}) {
    const token = cookie.get("USOSFIX_TOKEN");
    if (token != null && token != undefined) {
      const username = state.username; 
      await axios.put(
        `Account/SetUsername?token=${token}&username=${escape(username)}`
      ).then(() => {
        commit("setSuccessMsg", "Your username was saved");
      }).catch((handled) => {
        if(!handled) {
          commit("setErrorMsg", "There was an error while setting your username. Try again later...");
        }
      });
    } else {
      router.push({ name: "Login", params: { msg: "Session expired", type: "expired" } });
    }
  },
  async changeUsername({commit}, name) {
    commit("setUsername", name);
  },
  async addLeader({commit}, id) {
    commit("setLoadingLeaders", true);
    const token = cookie.get("USOSFIX_TOKEN");
    if (token != null && token != undefined) {
      await axios.put(
        `Roles/SetRoleByStudentNumber?token=${token}&studentNumber=${id}&roleString=Leader`
      ).then(async () => {
        await axios.get(
          `Roles/AdminsAndLeaders?token=${token}`
        ).then((data2) => {
          commit("setLeaders", data2.filter(x => x.role == "Leader"));
        }).catch((handled) => {
          if(!handled) {
            commit("setErrorMsg", "There was an error while getting admins and leaders. Try again later...");
          }
        });
      }).catch((handled) => {
        if(!handled) {
          commit("setErrorMsg", "There was an error while adding leader. Try again later...");
        }
      });
      commit("setLoadingLeaders", false);
    } else {
      commit("logOut");
      router.push({ name: "Login", params: { msg: "Session expired", type: "expired" } });
    }
  },
  async removeLeader({commit}, id) {
    commit("setLoadingLeaders", true);
    const token = cookie.get("USOSFIX_TOKEN");
    if (token != null && token != undefined) {
      await axios.put(
        `Roles/SetRoleByStudentNumber?token=${token}&studentNumber=${id}&roleString=User`
      ).then(async () => {
        await axios.get(
          `Roles/AdminsAndLeaders?token=${token}`
        ).then((data2) => {
          commit("setLeaders", data2.filter(x => x.role == "Leader"));
        }).catch((handled) => {
          if(!handled) {
            commit("setErrorMsg", "There was an error while getting admins and leaders. Try again later...");
          }
        });
      }).catch((handled) => {
        if(!handled) {
          commit("setErrorMsg", "There was an error while removing leader. Try again later...");
        }
      });
      commit("setLoadingLeaders", false);
    } else {
      commit("logOut");
      router.push({ name: "Login", params: { msg: "Session expired", type: "expired" } });
    }
  },
  async addAdmin({commit}, id) {
    commit("setLoadingAdmins", true);
    const token = cookie.get("USOSFIX_TOKEN");
    if (token != null && token != undefined) {
      await axios.put(
        `Roles/SetRoleByStudentNumber?token=${token}&studentNumber=${id}&roleString=Admin`
      ).then(async () => {
        await axios.get(
          `Roles/AdminsAndLeaders?token=${token}`
        ).then((data2) => {
          commit("setAdmins", data2.filter(x => x.role == "Admin"));
        }).catch((handled) => {
          if(!handled) {
            commit("setErrorMsg", "There was an error while getting admins and leaders. Try again later...");
          }
        });
      }).catch((handled) => {
        if(!handled) {
          commit("setErrorMsg", "There was an error while adding admin. Try again later...");
        }
      });
      commit("setLoadingAdmins", false);
    } else {
      commit("logOut");
      router.push({ name: "Login", params: { msg: "Session expired", type: "expired" } });
    }
  },
  async removeAdmin({commit}, id) {
    commit("setLoadingAdmins", true);
    const token = cookie.get("USOSFIX_TOKEN");
    if (token != null && token != undefined) {
      await axios.put(
        `Roles/SetRoleByStudentNumber?token=${token}&studentNumber=${id}&roleString=User`
      ).then(async () => {
        await axios.get(
          `Roles/AdminsAndLeaders?token=${token}`
        ).then((data2) => {
          commit("setAdmins", data2.filter(x => x.role == "Admin"));
        }).catch((handled) => {
          if(!handled) {
            commit("setErrorMsg", "There was an error while getting admins and leaders. Try again later...");
          }
        });
      }).catch((handled) => {
        if(!handled) {
          commit("setErrorMsg", "There was an error while removing admin. Try again later...");
        }
      });
      commit("setLoadingAdmins", false);
    } else {
      commit("logOut");
      router.push({ name: "Login", params: { msg: "Session expired", type: "expired" } });
    }
  },
  async sendToFaculty({commit}, ids) {
    const token = cookie.get("USOSFIX_TOKEN");
    if (token != null && token != undefined) {
      await axios.post(
        `Admin/EndExchangeWindow?token=${token}`, ids
      ).then(() => {
        commit("setSuccessMsg", "Mail to faculty was sent");
      }).catch((handled) => {
        if(!handled) {
          commit("setErrorMsg", "There was an error while sending email to faculty. Try again later...");
        }
      });
    } else {
      commit("logOut");
      router.push({ name: "Login", params: { msg: "Session expired", type: "expired" } });
    }
  },
  async revealYourself({commit}, reveal) {
    const token = cookie.get("USOSFIX_TOKEN");
    if (token != null && token != undefined) {
      await axios.put(
        `Account/SetVisibility?token=${token}&visible=${reveal}`
      ).then(() => {
        commit("setRevealed", reveal);
      }).catch((handled) => {
        if(!handled) {
          commit("setErrorMsg", "There was an error while sending request. Try again later...");
        }
      });
    } else {
      commit("logOut");
      router.push({ name: "Login", params: { msg: "Session expired", type: "expired" } });
    }
  },
  async startNewSemester({commit}) {
    const token = cookie.get("USOSFIX_TOKEN");
    if (token != null && token != undefined) {
      await axios.post(
        `Admin/ChangeToNextTerm?token=${token}`
      ).then(() => {
        commit("setSuccessMsg", "New semster was started");
      }).catch((handled) => {
        if(!handled) {
          commit("setErrorMsg", "There was an error while sending request to start new semester. Try again later...");
        }
      });
    } else {
      commit("logOut");
      router.push({ name: "Login", params: { msg: "Session expired", type: "expired" } });
    }
  },
  async endExchangeWindow({commit}, ids) {
    const token = cookie.get("USOSFIX_TOKEN");
    if (token != null && token != undefined) {
      await axios.put(
        `Exchanges/RealizeExchangesInSubjects?token=${token}`, ids
      ).then(() => {
        commit("setSuccessMsg", "Exchanges were calculated");
      }).catch((handled) => {
        if(!handled) {
          commit("setErrorMsg", "There was an error while calculating exchanges. Try again later...");
        }
      });
    } else {
      commit("logOut");
      router.push({ name: "Login", params: { msg: "Session expired", type: "expired" } });
    }
  },
};

const mutations = {
  setName: (state, name) => state.name = name,
  setSurname: (state, surname) => state.surname = surname,
  setUsername: (state, username) => state.username = username,
  setId: (state, id) => state.id = id,
  setRole: (state, role) => state.role = role,
  setLeaders: (state, leaders) => state.leaders = leaders,
  setAdmins: (state, admins) => state.admins = admins,
  setIndex: (state, index) => state.index =index,
  setRevealed: (state, revealed) => state.revealed = revealed,
  setLoadingLeaders: (state, value) => state.loadingLeaders = value,
  setLoadingAdmins: (state, value) => state.loadingAdmins = value,
  setLoadingSubjects: (state, value) => state.loadingSubjects = value,
};

export default {
  state,
  getters,
  actions,
  mutations
};
