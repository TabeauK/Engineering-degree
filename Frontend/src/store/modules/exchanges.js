import axios from "axios";
import cookie from "vue-cookies";
import router from "../../router/index";

const state = {
  exchanges: [],
  loadingExchanges: true,
};

const getters = {
  getExchanges: state => state.exchanges,
  getLoadingExchanges: state => state.loadingExchanges,
};

const actions = {
  async fetchExchanges({commit}) {
    commit("setLoadingExchanges", true);
    const token = cookie.get("USOSFIX_TOKEN");
    if (token != null && token != undefined) {
      await axios.get(
        `Exchanges/Relations?token=${token}`
      ).then(async (response) => {
        await axios.get(
          `Exchanges/Exchanges?token=${token}`
        ).then((response2) => {
          let exchanges = [];
          response2.forEach(exchange => {
            if(exchange.exchangeState === "Submitted") {
              let relations = response.filter(x => x.exchanges.some(y => y.id == exchange.id));
              exchanges.push({
                id: exchange.id,
                groupTo: exchange.groupTo,
                groupFrom: exchange.groupFrom,
                relations: relations.map(relation => {
                  return {
                    type: relation.type,
                    id: relation.id,
                    relationWith: relation.exchanges.filter(x => x.id != exchange.id),
                  };
                }),
              });
            }
          });
          commit("setExchanges", exchanges);
        }).catch((handled) => {
          if(!handled) {
            commit("setErrorMsg", "There was an error while getting exchanges. Try again later...");
          }
        });
      }).catch((handled) => {
        if(!handled) {
          commit("setErrorMsg", "There was an error while getting exchanges relations. Try again later...");
        }
      });
      commit("setLoadingExchanges", false);
    } else {
      commit("logOut");
      router.push({ name: "Login", params: { msg: "Session expired", type: "expired" } });
    }
  },
  async addRelation({commit}, args) {
    commit("setLoadingExchanges", true);
    const token = cookie.get("USOSFIX_TOKEN");
    if (token != null && token != undefined) {
      await axios.put(
        `Exchanges/AddRelation?token=${token}&exchange1Id=${args.fromId}&exchange2Id=${args.toId}&relationType=${args.type}`
      ).then(async () => {
        await actions.fetchExchanges({commit});
      }).catch((handled) => {
        if(!handled) {
          commit("setErrorMsg", "There was an error while adding relations between exchanges. Try again later...");
        }
      });
    } else {
      commit("logOut");
      router.push({ name: "Login", params: { msg: "Session expired", type: "expired" } });
    }
  },
  async removeRelation({commit}, id) {
    commit("setLoadingExchanges", true);
    const token = cookie.get("USOSFIX_TOKEN");
    if (token != null && token != undefined) {
      await axios.put(
        `Exchanges/DeleteRelation?token=${token}&relationId=${id}`
      ).then(async () => {
        await actions.fetchExchanges({commit});
      }).catch((handled) => {
        if(!handled) {
          commit("setErrorMsg", "There was an error while removing relations between exchanges. Try again later...");
        }
      });
    } else {
      commit("logOut");
      router.push({ name: "Login", params: { msg: "Session expired", type: "expired" } });
    }
  },
};

const mutations = {
  setExchanges: (state, exchanges) => state.exchanges = exchanges,
  setLoadingExchanges: (state, value) => state.loadingExchanges = value,
};

export default {
  state,
  getters,
  actions,
  mutations
};
