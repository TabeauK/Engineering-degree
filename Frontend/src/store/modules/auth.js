import axios from "axios";
import cookie from "vue-cookies";
import router from "../../router/index";

const state = {
  logged: false, //dev
};

const getters = {
  isLogged: state => state.logged,
};

const actions = {
  async login() {
    await axios.get(
      "UsosAuthorization/OauthToken?env=Web"
    ).then((response) => {
      const token = response.token;
      const secret = response.secret;
      cookie.set("USOSFIX_TOKEN", token, { expires: "15m" });
      cookie.set("USOSFIX_SECRET", secret, { expires: "15m" });
      window.location = `https://apps.usos.pw.edu.pl/apps/authorize?oauth_token=${token}&interactivity=minimal`;
    }).catch((handled) => {
      if(!handled) {
        router.push({ name: "Login", params: { msg: "Something went wrong. Try again later...", type: "error" }});
      }
    });
  },
  async verifyToken({ commit }, params) {
    const token = params.token;
    const pin = params.pin;
    if(token !== undefined && token !== null && pin !== null && pin !== undefined) {
      if(cookie.get("USOSFIX_TOKEN") !== token) {
        commit("logOut");
        router.push({ name: "Login", params: { msg: "Session expired", type: "expired" }});
      } else {
        await axios.get(
          `UsosAuthorization/AccessToken?pin=${pin}&token=${token}`
        ).then((response) => {
          cookie.remove("USOSFIX_SECRET");
          cookie.set("USOSFIX_TOKEN", response.token, { expires: "2h" });
          commit("logIn");
          router.push({ name: "Timetable"});
        }).catch((handled) => {
          if(!handled) {
            router.push({ name: "Login", params: { msg: "Something went wrong. Try again later...", type: "error" }});
          }
        });
      }
    } else if(cookie.get("USOSFIX_TOKEN")) {
      await axios.get(
        `UsosAuthorization/IsTokenValid?token=${cookie.get("USOSFIX_TOKEN")}`
      ).then(() => {
        cookie.set("USOSFIX_TOKEN", cookie.get("USOSFIX_TOKEN"), { expires: "2h" });
        commit("logIn");
        router.push({ name: "Timetable"});
      }).catch((handled) => {
        if(!handled) {
          router.push({ name: "Login", params: { msg: "Something went wrong. Try again later...", type: "error" }});
        }
      });
    } else {
      router.push({ name: "Login", params: { msg: "", type: "" }});
    }
  },
  async logout({commit}) {
    const token = cookie.get("USOSFIX_TOKEN");
    if (token != null && token != undefined) {
      await axios.delete(
        `UsosAuthorization/LogOff?token=${token}`
      );
    }
    cookie.remove("USOSFIX_TOKEN");
    commit("logOut");
    router.push({ name: "Login", params: { msg: "You have been logged out", type: "logout" } });
  },
};

const mutations = {
  logIn: (state) => (state.logged = true),
  logOut: (state) => (state.logged = false)
};

export default {
  state,
  getters,
  actions,
  mutations
};
