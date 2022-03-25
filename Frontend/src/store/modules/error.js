const state = {
  errorMsg: "",
  successMsg: "",
  warnMsg: "",
  infoMsg: "",
};

const getters = {
  getErrorMsg: state => state.errorMsg,
  getSuccessMsg: state => state.successMsg,
  getWarnMsg: state => state.warnMsg,
  getInfoMsg: state => state.infoMsg,
};

const actions = {
  clearError({commit}) {
    commit("setErrorMsg", "");
  },
  clearSuccess({commit}) {
    commit("setSuccessMsg", "");
  },
  clearWarn({commit}) {
    commit("setWarnMsg", "");
  },
  clearInfo({commit}) {
    commit("setInfoMsg", "");
  },
};

const mutations = {
  setErrorMsg: (state, msg) => state.errorMsg = msg,
  setSuccessMsg: (state, msg) => state.successMsg = msg,
  setWarnMsg: (state, msg) => state.warnMsg = msg,
  setInfoMsg: (state, msg) => state.infoMsg = msg,
};

export default {
  state,
  getters,
  actions,
  mutations
};
