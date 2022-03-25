import axios from "axios";
import cookie from "vue-cookies";
import router from "../../router/index";
import ApiPath from "../../config";

const state = {
  "suggestions": [],
  "connection": "",
  "selectedContactId": null,
  "lastChatDate": "",
  "isThisFirstChat": false,
  "chats": [],
};

const getters = {
  getMessages: state => state.selectedContactId === null ? [] : state.chats.find(x => x.id == state.selectedContactId).messages,
  getSelectedContactId: state => state.selectedContactId === null ? state.chats[0] !== undefined ? state.chats[0].id : null : state.selectedContactId,
  isAccepted: state => state.selectedContactId === null ? false : state.chats.find(x => x.id == state.selectedContactId).accepted,
  isAcceptedBack: state => state.selectedContactId === null ? false : state.chats.find(x => x.id == state.selectedContactId).acceptedBack,
  isThisFirstMsg: state => state.selectedContactId === null ? false : state.chats.find(x => x.id == state.selectedContactId).isThisFirstMsg,
  isThisFirstChat: state => state.isThisFirstChat,
  isLoading: state => state.selectedContactId === null ? false : state.chats.find(x => x.id == state.selectedContactId).loading,
  getMessageInput: state => state.selectedContactId === null ? "" : state.chats.find(x => x.id == state.selectedContactId).messageInput,
  getUser: state => state.selectedContactId === null ? {} : state.chats.find(x => x.id == state.selectedContactId).participants[0].user,
  getChats: state => {
    state.chats.sort((x,y) => y.messages[y.messages.length - 1].date - x.messages[x.messages.length - 1].date);
    var chats = [];
    for(let i = 0; i < state.chats.length; i++){
      chats.push({
        id: state.chats[i].id,
        name: state.chats[i].participants[0].user,
        newMessage: state.chats[i].newMessage,
        lastMessage: state.chats[i].messages[state.chats[i].messages.length - 1],
      });
    }
    return chats;
  },
  getSuggestions: state => state.suggestions,
};

const actions = {
  async fetchChats({commit}, myId) {
    const token = cookie.get("USOSFIX_TOKEN");
    if(token != null && token != undefined) {
      await axios.get(
        `Messages/ConversationsSince?token=${token}&date=${state.lastChatDate == "" ? "" : new Date(state.lastChatDate).toJSON()}`
      ).then(async (response) => {
        var chats = [];
        response.forEach(chat => {
          const msg = {
            content: chat.messages[chat.messages.length - 1].content,
            from: chat.messages[chat.messages.length - 1].authorId,
            type: chat.messages[chat.messages.length - 1].type,
            date: new Date(chat.messages[chat.messages.length - 1].sentAt).getTime(),
            shortDate: getDate(chat.messages[chat.messages.length - 1].sentAt),
            time: getTime(chat.messages[chat.messages.length - 1].sentAt),
            sentAt: chat.messages[chat.messages.length - 1].sentAt,
          };
          chats.push({
            messageInput: "",
            id: chat.id,
            acceptedBack: !chat.participants.some(x => x.state == "Invited" || x.state == "Rejected"),
            accepted: chat.participants.find(x => x.user.id == myId).state == "Author" || chat.participants.find(x => x.user.id == myId).state == "Accepted",
            participants: chat.participants.filter(x => x.user.id != myId),
            messages: [msg],
            loading: false,
            isThisFirstMsg: false,
            newMessage: 0,
            dummy: false,
          });
        });
        commit("addChats", chats);
        commit("setSelectedContactId", null);
        await actions.fetchMessages({commit});
      }).catch((handled) => {
        if(!handled) {
          commit("setErrorMsg", "There was an error while getting chats. Try again later...");
        }
      });
    } else {
      commit("logOut");
      router.push({ name: "Login", params: { msg: "Session expired", type: "expired" } });
    }
  },
  async fetchMessages({commit}, id = null) {
    if(id == null) {
      if(state.selectedContactId === undefined || state.selectedContactId === null) {
        return;
      }
      id = state.selectedContactId;
    } else if((typeof id === "string" || id instanceof String) && id.includes("dummy")) {
      return;
    }
    if(!state.chats.find(x => x.id == id).loading) {
      commit("setLoading", {id, loading: true});
      const token = cookie.get("USOSFIX_TOKEN");
      const date = state.chats.find(x => x.id == id).messages[state.chats.find(x => x.id == id).messages.length - 1].date;
      if (token != null && token != undefined) {
        await axios.get(
          `Messages/MessagesSince?token=${token}&date=${date == "" ? "" : new Date(date).toJSON()}&conversationId=${id}`
        ).then((response) => {
          var msgs = [];
          response.messages.forEach(msg => {
            msgs.push({
              content: msg.content,
              from: msg.authorId,
              type: msg.type,
              date: new Date(msg.sentAt).getTime(),
              shortDate: getDate(msg.sentAt),
              time: getTime(msg.sentAt),
              sentAt: msg.sentAt,
            });
          });
          commit("addMessages", {id, msgs});
          commit("setLoading", {id, loading: false});
        }).catch((handled) => {
          if(!handled) {
            commit("setErrorMsg", "There was an error while getting messages. Try again later...");
          }
        });
        commit("setLoading", {id, loading: false});
      } else {
        router.push({ name: "Login", params: { msg: "Session expired", type: "expired" } });
      }
    }
  },
  async changeSetSelectedContactId({commit}, nr) {
    commit("setSelectedContactId", nr);
  },
  async changeMessageInput({commit}, input) {
    commit("setMessageInput", input);
  },
  async pushMessage({commit}, content) {
    if(content === "") {
      return;
    }
    const id = state.selectedContactId;
    commit("setLoading", {id, loading: true});
    const token = cookie.get("USOSFIX_TOKEN");
    if (token != null && token != undefined) {
      await state.connection.invoke("Send", content, id).then(() => {
        commit("clearMessageInput", id);
        commit("setLoading", {id, loading: false});
      });
    } else {
      router.push({ name: "Login", params: { msg: "Session expired", type: "expired" } });
    }
  },
  async inviteUserToChat({commit}, {id, myId} ) {
    const token = cookie.get("USOSFIX_TOKEN");
    if (token != null && token != undefined) {
      await axios.post(
        `Messages/InviteToChatById?token=${token}&userToInviteId=${id}`
      ).then(async () => {
        commit("setSuccessMsg", "User invited");
        actions.fetchChats({ commit }, myId);
        actions.fetchMessages({ commit });
      }).catch((handled) => {
        if(!handled) {
          commit("setErrorMsg", "There was an error while sending invite. Try again later...");
        }
      });
    } else {
      router.push({ name: "Login", params: { msg: "Session expired", type: "expired" } });
    }
  },
  async acceptUser({commit}, myId) {
    const id = state.selectedContactId;
    const token = cookie.get("USOSFIX_TOKEN");
    if (token != null && token != undefined) {
      await axios.put(
        `Messages/AcceptChat?token=${token}&conversationId=${id}`
      ).then(() => {
        commit("acceptChat", id);
        actions.fetchChats({ commit }, myId);
        actions.fetchMessages({ commit });
      }).catch((handled) => {
        if(!handled) {
          commit("setErrorMsg", "There was an error while accepting invite. Try again later...");
        }
      });
    } else {
      router.push({ name: "Login", params: { msg: "Session expired", type: "expired" } });
    }
  },
  async fetchSuggestions({commit}, prefix) {
    const token = cookie.get("USOSFIX_TOKEN");
    if (token != null && token != undefined) {
      await axios.get(
        `Account/UserSearch?token=${token}&prefix=${prefix}`
      ).then((suggestions) => commit("updateSuggestions", suggestions)).catch((handled) => {
        if(!handled) {
          commit("setErrorMsg", "There was an error while getting users. Try again later...");
        }
      });
    } else {
      router.push({ name: "Login", params: { msg: "Session expired", type: "expired" } });
    }
  },
  async rejectUser({commit}, myId) {
    const id = state.selectedContactId;
    const token = cookie.get("USOSFIX_TOKEN");
    if (token != null && token != undefined) {
      await axios.put(
        `Messages/RejectChat?token=${token}&conversationId=${id}`
      ).then(() => {
        commit("rejectUser", id);
        actions.fetchChats({ commit }, myId);
        actions.fetchMessages({ commit });
      }).catch((handled) => {
        if(!handled) {
          commit("setErrorMsg", "There was an error while rejecting invite. Try again later...");
        }
      });
    } else {
      router.push({ name: "Login", params: { msg: "Session expired", type: "expired" } });
    }
  },
  async setConnection({commit}, myId) {
    if(state.connection != "") {
      return;
    }
    const token = cookie.get("USOSFIX_TOKEN");
    if (token != null && token != undefined) {
      const signalR = require("@microsoft/signalr");
      let connection = new signalR.HubConnectionBuilder()
        .withUrl(`${ApiPath()}chat?token=${token}`)
        .withAutomaticReconnect()
        .build();

      connection.on("Receive", async data => {
        const msg = {
          content: data.content,
          from: data.authorId,
          type: data.type,
          date: new Date(data.sentAt).getTime(),
          shortDate: getDate(data.sentAt),
          time: getTime(data.sentAt),
          sentAt: data.sentAt,
        };
        let index = state.chats.findIndex(x => x.id == data.conversationId);
        if(index == -1) {
          await axios.get(
            `Messages/ConversationsSince?token=${token}&date=`
          ).then(async (response) => {
            let chat = response.find(chat => chat.id == data.conversationId);
            let cha = {
              messageInput: "",
              id: chat.id,
              acceptedBack: !chat.participants.some(x => x.state == "Invited" || x.state == "Rejected"),
              accepted: chat.participants.find(x => x.user.id == myId).state == "Author" || chat.participants.find(x => x.user.id == myId).state == "Accepted",
              participants: chat.participants.filter(x => x.user.id != myId),
              messages: [msg],
              loading: false,
              isThisFirstMsg: false,
              newMessage: 1,
              dummy: false,
            };
            commit("addChats", [cha]);
          }).catch((handled) => {
            if(!handled) {
              commit("setErrorMsg", "There was an error while getting chats. Try again later...");
            }
          });
        } else {
          commit("pushMessage", { id: data.conversationId, msg});
        }
      });

      connection.start()
        .then(() => commit("setConnection", connection));
    } else {
      router.push({ name: "Login", params: { msg: "Session expired", type: "expired" } });
    }
  },
};

const mutations = {
  setConnection: (state, conn) => state.connection = conn,
  setSelectedContactId: (state, id) => { 
    if(id === null && state.selectedContactId === null) {
      state.chats.sort((x,y) => y.messages[y.messages.length - 1].date - x.messages[x.messages.length - 1].date);
      state.selectedContactId = state.chats[0].id;
    } else {
      state.selectedContactId = id;
      state.chats[state.chats.findIndex(x=> x.id == id)].newMessage = 0;
    }
  },
  setMessageInput: (state, input) => state.chats.find(x => x.id == state.selectedContactId).messageInput = input,
  addMessages: (state, props) => {
    props.msgs.forEach(msg => state.chats.find(x=> x.id == props.id).messages.unshift(msg));
    state.chats.find(x=> x.id == props.id).isThisFirstMsg = props.msgs.length < 50;
  },
  addChats: (state, chats) => {
    state.chats.isThisFirstChat = chats.length < 50;
    chats = chats.filter(x => !state.chats.some(y => y.id == x.id));
    chats.forEach(chat => state.chats.unshift(chat));
    state.chats.sort((x,y) => y.messages[y.messages.length - 1].date - x.messages[x.messages.length - 1].date);
    state.lastChatDate = state.chats[state.chats.length -1].messages[state.chats[state.chats.length -1].messages.length - 1].date;
  },
  acceptChat: (state, id) => state.chats.find(x=> x.id == id).accepted = true,
  rejectUser: (state, id) => state.chats.splice(state.chats.findIndex(x=> x.id == id), 1),
  clearMessageInput: (state, id) => state.chats[state.chats.findIndex(x=> x.id == id)].messageInput = "",
  setLoading: (state, props) => state.chats[state.chats.findIndex(x=> x.id == props.id)].loading = props.loading,
  pushMessage: (state, props) => {
    let chat = state.chats.find(x => x.id == props.id);
    let currentChatDate = state.chats.find(x => x.id == state.selectedContactId).messages[state.chats.find(x => x.id == state.selectedContactId).messages.length - 1].date;
    let targetChatDate = chat.messages[chat.messages.length - 1].date;
    if(state.selectedContactId == props.id) {
      var elem = document.getElementsByClassName("msg_history")[0];
      state.chats.find(x => x.id == state.selectedContactId).messages.push(props.msg);
      elem.scrollTop = elem.scrollHeight;
    } else if(currentChatDate > targetChatDate) {
      chat.messages.push(props.msg);
      chat.newMessage++;
    } else {
      chat.messages.push(props.msg);
      chat.newMessage++;
    }
  },
  updateSuggestions: (state, suggestions) => {
    var arr = [];
    suggestions.forEach(x => {
      arr.push({name: x.displayName + "#" + x.id.toLocaleString("en-US", {
        minimumIntegerDigits: 4,
        useGrouping: false
      }), value: x.id});
    });
    state.suggestions = arr;
  }
};

export default {
  state,
  getters,
  actions,
  mutations
};

function getTime(str) {
  var date = new Date(str);
  var minutes = date.getMinutes();
  var hour = date.getHours();
  if(minutes < 10) {
    minutes = "0" + minutes;
  }
  if(hour < 10) {
    hour = "0" + hour;
  }
  return hour + ":" + minutes;
}

function getDate(str) {
  var date = new Date(str);
  var year = date.getFullYear();
  var month = date.getMonth() + 1; // beware: January = 0; February = 1, etc.
  var day = date.getDate();
  if(day < 10){
    day = "0" + day;
  }
  if(month < 10) {
    month = "0" + month;
  }
  return day + "-" + month + "-" + year;
}
