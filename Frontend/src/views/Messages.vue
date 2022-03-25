<template>
  <div class="messaging p-shadow-24">
    <Toast position="top-right" />
    <div class="inbox_msg">
      <div class="inbox_people">
        <div class="headind_srch">
          <div
            v-if="!isMobile"
            style="float: left;"
          >
            <AutoComplete
              v-model="userToAdd"
              :suggestions="getSuggestions"
              :placeholder="getString('Type in a username...')"
              field="name"
              @complete="{ selected = false; fetchSuggestions(userToAdd); }"
              @item-select="selected = true"
              @item-unselect="selected = false"
            />
          </div>
          <div
            v-if="!isMobile"
            style="float: right;"
          >
            <Button
              v-if="selected"
              :label="getString('Invite')"
              class="p-button-success"
              @click="inviteUser"
            />
            <Button
              v-else
              :label="getString('Invite')"
              class="p-button-success"
              disabled="disabled"
              @click="inviteUser"
            />
          </div>
          <div v-else>
            <Button
              class="p-button-info"
              icon="pi pi-plus"
              @click="display = true"
            />
          </div>
          <Dialog
            v-if="isMobile"
            v-model:visible="display"
            :header="getString('Invite')"
            position="top"
          >
            <div class="p-fluid">
              <AutoComplete
                v-model="userToAdd"
                :suggestions="getSuggestions"
                :placeholder="getString('Type in a username...')"
                field="name"
                @complete="{ selected = false; fetchSuggestions(userToAdd); }"
                @item-select="selected = true"
                @item-unselect="selected = false"
              />
            </div>
            <div>
              <Button
                v-if="selected"
                :label="getString('Invite')"
                class="p-button-success"
                @click="inviteUser"
              />
              <Button
                v-else
                :label="getString('Invite')"
                class="p-button-success"
                disabled="disabled"
              />
            </div>
          </Dialog>
        </div>
        <div class="inbox_chat">
          <div
            v-for="chat in getChats"
            :key="chat.id"
            :class="
              'chat_list ' +
                (chat.id == getSelectedContactId ? 'active_chat' : '')"
            @click="() => changeChat(chat.id)"
          >
            <div class="chat_people">
              <div class="chat_ib">
                <Badge
                  v-if="chat.newMessage > 0 && isMobile"
                  :value="chat.newMessage" 
                  severity="warning"
                />
                <h5>
                  {{ chat.name.displayName + "#" + chat.name.id.toLocaleString("en-US", {
                    minimumIntegerDigits: 4,
                    useGrouping: false
                  })
                  }}
                  <span
                    v-if="!isMobile"
                    class="chat_date"
                  >{{ chat.lastMessage.time + " " + chat.lastMessage.shortDate }}
                  </span>
                </h5>
                <p
                  v-if="isMobile"
                  class="chat_date"
                >
                  {{ chat.lastMessage.time + " " + chat.lastMessage.shortDate }}
                </p>
                <div>
                  <Badge
                    v-if="chat.newMessage > 0 && !isMobile"
                    :value="chat.newMessage" 
                    severity="warning"
                  />
                  <p 
                    v-if="chat.lastMessage.type == 'Info'"
                  >
                    {{ getLang(chat.lastMessage.content) }}
                  </p>
                  <p 
                    v-if="chat.lastMessage.type == 'Invite'"
                  >
                    {{
                      isMobile ? "" :
                      chat.lastMessage.from == getId
                        ? getString("you") + ":"
                        : (chat.name.displayName + "#" + chat.name.id.toLocaleString("en-US", {
                          minimumIntegerDigits: 4,
                          useGrouping: false
                        }), + ":")
                    }}
                    {{ getLang(chat.lastMessage.content) }}
                  </p>
                  <p 
                    v-else 
                  >
                    {{
                      chat.lastMessage.from == getId
                        ? getString("you")
                        : chat.name.displayName + "#" + chat.name.id.toLocaleString("en-US", {
                          minimumIntegerDigits: 4,
                          useGrouping: false
                        })
                    }}: {{ getLang(chat.lastMessage.content) }}
                  </p>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
      <div class="mesgs">
        <div class="msg_history">
          <div
            v-for="msg in getMessages"
            :key="msg.sentAt"
          >
            <div
              v-if="msg.type == 'Invite' && !isAccepted"
              class="info_msg"
            >
              <Panel :header="getString('Invitation')">
                <div style="padding-bottom: 0.5em;">
                  {{ getLang(msg.content) }}
                </div>
                <Button
                  :label="getString('Accept')"
                  class="p-button-success"
                  @click="acceptUser(getId)"
                />
                <Button
                  :label="getString('Reject')"
                  class="p-button-danger"
                  @click="rejectUser(getId)"
                />
              </Panel>
            </div>
            <div
              v-else-if="msg.type == 'Info' || msg.type == 'Invite'"
              class="info_msg"
            >
              <p>{{ getLang(msg.content) }}</p>
            </div>
            <div
              v-else-if="msg.type == 'Normal' && msg.from == getId"
              class="outgoing_msg"
            >
              <div class="sent_msg">
                <p>{{ getLang(msg.content) }}</p>
                <span class="time_date"> {{ msg.time }} | {{ msg.shortDate }}</span>
              </div>
            </div>
            <div
              v-else-if="msg.type == 'Normal'"
              class="incoming_msg"
            >
              <div class="received_msg">
                <div class="received_withd_msg">
                  <p>{{ getLang(msg.content) }}</p>
                  <span class="time_date">
                    {{ msg.time }} | {{ msg.shortDate }}</span>
                </div>
              </div>
            </div>
          </div>
        </div>
        <BlockUI :blocked="isLoading">
          <div
            v-if="isAccepted && isAcceptedBack"
            class="type_msg"
          >
            <div class="input_msg_write">
              <input
                v-model="messageInput"
                type="text"
                class="write_msg"
                :placeholder="getString('Type a message')"
                @keyup.enter="pushMessage(messageInput)"
              >
              <Button
                v-if="messageInput.length > 0"
                icon="pi pi-send"
                class="p-button-rounded msg_send_btn"
                @click="pushMessage(messageInput)"
              />
              <Button
                v-else
                icon="pi pi-send"
                class="p-button-rounded msg_send_btn"
                disabled
              />
            </div>
          </div>
        </BlockUI>
        <ProgressBar
          v-if="isLoading"
          mode="indeterminate"
          style="height: .5em;"
        />
      </div>
    </div>
  </div>
</template>

<script>
import { mapGetters } from "vuex";
import { mapActions } from "vuex";
import Button from "primevue/button";
import Toast from "primevue/toast";
import ProgressBar from "primevue/progressbar";
import BlockUI from "primevue/blockui";
import Panel from "primevue/panel";
import Dialog from "primevue/dialog";
import AutoComplete from "primevue/autocomplete";
import Badge from "primevue/badge";

export default {
  name: "Messages",
  components: {
    Button,
    Toast,
    ProgressBar,
    BlockUI,
    Panel,
    Dialog,
    AutoComplete,
    Badge
  },
  data() {
    return {
      selected: false,
      userToAdd: null,
      chatNotChanged: false,
      isMobile: (screen.width <= 620),
      display: false,
    };
  },
  computed: {
    ...mapGetters([
      "getInvitations",
      "getSelectedContactId",
      "isThisFirstMsg",
      "isThisFirstChat",
      "getMessageInput",
      "getChats",
      "getUser",
      "getMessages",
      "getId",
      "isAccepted",
      "isAcceptedBack",
      "isLoading",
      "getString",
      "getLang",
      "getSuggestions"
    ]),
    messageInput: {
      get() {
        return this.getMessageInput;
      },
      set(value) {
        this.changeMessageInput(value);
      }
    },
  },
  created() {
    this.fetchChats(this.getId);
    this.setConnection(this.getId);
    this.isMobile = screen.width <= 860;
    window.addEventListener("resize", () => this.isMobile = screen.width <= 860);
  },
  mounted() {
    var elem = this.$el.getElementsByClassName("msg_history")[0];
    elem.scrollTop = elem.scrollHeight;
    elem.addEventListener("scroll", e => {
      if (e.target.scrollTop == 0 && !this.isThisFirstMsg) {
        this.chatNotChanged = true;
        this.fetchMessages();
      }
    });
    this.$el
      .getElementsByClassName("inbox_chat")[0]
      .addEventListener("scroll", e => {
        if (
          e.target.scrollTop == e.target.scrollHeight &&
          !this.isThisFirstChat
        ) {
          this.chatNotChanged = true;
          this.fetchChats(this.getId);
        }
      });
  },
  updated() {
    var elem = this.$el.getElementsByClassName("msg_history")[0];
    if (elem && !this.chatNotChanged) {
      elem.scrollTop = elem.scrollHeight;
      if (elem.scrollTop == 0 && !this.isThisFirstMsg) {
        this.fetchMessages();
      }
    }
    this.chatNotChanged = false;
  },
  methods: {
    ...mapActions([
      "fetchChats",
      "changeSetSelectedContactId",
      "fetchMessages",
      "changeMessageInput",
      "pushMessage",
      "inviteUserToChat",
      "acceptUser",
      "rejectUser",
      "setConnection",
      "fetchSuggestions"
    ]),
    inviteUser() {
      if (!this.selected || this.userToAdd == null || this.userToAdd.value == null ) {
        this.$toast.add({
          severity: "error",
          summary: this.getString("User must be selected from list"),
          life: 3000
        });
      } else {
        this.inviteUserToChat({id: this.userToAdd.value, myId: this.getId }).then(
          () => (this.userToAdd = null)
        );
        this.display = false;
      }
    },
    changeChat(id) {
      this.changeSetSelectedContactId(id);
    }
  }
};
</script>

<style>
.p-autocomplete-panel,
.p-autocomplete-items,
.p-autocomplete-item {
  text-align: left !important;
}

.p-datatable-scrollable-header,
.p-datatable-scrollable-footer {
  overflow: visible;
}
</style>

<style scoped>
.p-autocomplete-panel,
.p-autocomplete-items,
.p-autocomplete-item {
  text-align: left !important;
}

button {
  margin-right: 0.5rem;
}

.messaging {
  height: 100%;
  margin: 0 calc(50% - 500px);
}

img {
  max-width: 100%;
}

.inbox_people {
  background: #f8f8f8 none repeat scroll 0 0;
  float: left;
  overflow: hidden;
  width: 40%;
  border-right: 1px solid #c4c4c4;
  max-width: 400px;
  height: 100%;
}

.inbox_msg {
  height: 100%;
  border: 1px solid #c4c4c4;
  clear: both;
  overflow: hidden;
}

.top_spac {
  margin: 20px 0 0;
}

.recent_heading {
  float: left;
  width: 40%;
}

.headind_srch {
  padding: 10px 29px 10px 20px;
  border-bottom: 1px solid #c4c4c4;
  height: 60px;
}

.srch_bar {
  display: inline-block;
  text-align: right;
  width: 60%;
}

.recent_heading h4 {
  color: #2196f3;
  font-size: 21px;
  margin: auto;
}

.srch_bar input {
  border: 1px solid #cdcdcd;
  border-width: 0 0 1px 0;
  width: 80%;
  padding: 2px 0 4px 6px;
  background: none;
}

.srch_bar .input-group-addon button {
  background: rgba(0, 0, 0, 0) none repeat scroll 0 0;
  border: medium none;
  padding: 0;
  color: #707070;
  font-size: 18px;
}

.srch_bar .input-group-addon {
  margin: 0 0 0 -27px;
}

.chat_ib h5 {
  font-weight: 700;
  font-size: 17px;
  color: #464646;
  margin: 0 0 8px 0;
  text-align: left;
  overflow-wrap: break-word;
}

.chat_ib span {
  font-size: 13px;
  float: right;
}

.chat_ib p {
  font-size: 14px;
  color: #989898;
  margin: auto;
  text-align: left;
}

.chat_ib {
  cursor: default;
  float: left;
  padding: 0 0 0 15px;
  width: 100%;
}

.chat_people {
  overflow: hidden;
  clear: both;
}

.chat_list {
  border-bottom: 1px solid #c4c4c4;
  margin: 0;
  padding: 18px 16px 10px;
  height: 120px;
  text-overflow: ellipsis;
  overflow: hidden;
}

.inbox_chat {
  max-width: 400px;
  height: 100%;
  overflow-y: auto;
}

.active_chat {
  background: #ebebeb;
}

.received_withd_msg p {
  background: #ebebeb none repeat scroll 0 0;
  border-radius: 3px;
  color: #646464;
  font-size: 14px;
  margin: 0;
  padding: 5px 10px 5px 12px;
  width: 100%;
}

.time_date {
  color: #747474;
  display: block;
  font-size: 12px;
  margin: 8px 0 0;
}

.info_msg {
  display: inline-block;
  padding: 0 0 0 5px;
  vertical-align: top;
  width: 100%;
}

.info_msg p {
  border-radius: 3px;
  color: #646464;
  font-size: 12px;
  margin: 0;
  width: 100%;
}

.received_withd_msg {
  width: 57%;
}

.mesgs {
  padding: 30px 15px 0 25px;
  height: 100%;
  float: left;
  min-width: calc(100% - 400px);
  width: 60%;
  overflow: auto;
}

.sent_msg p {
  background: #2196f3 none repeat scroll 0 0;
  border-radius: 3px;
  font-size: 14px;
  margin: 0;
  color: #fff;
  padding: 5px 10px 5px 12px;
  width: 100%;
}

.outgoing_msg {
  overflow: hidden;
  margin: 15px 0 15px;
}

.sent_msg {
  float: right;
  width: 46%;
}

.input_msg_write input {
  background: rgba(0, 0, 0, 0) none repeat scroll 0 0;
  border: medium none;
  color: #4c4c4c;
  width: 100%;
  font-size: 15px;
  min-height: 50px;
  height: 50px;
}

.input_msg_write input:focus {
  outline: none;
}

.type_msg {
  border-top: 1px solid #c4c4c4;
  position: relative;
}

.msg_send_btn {
  background: #2196f3 none repeat scroll 0 0;
  border: medium none;
  border-radius: 50%;
  color: #fff;
  cursor: pointer;
  font-size: 17px;
  height: 33px;
  position: absolute;
  right: 0;
  top: 11px;
  width: 33px;
}

.msg_history {
  padding-right: 20px;
  height: calc(100% - 70px);
  overflow-y: scroll;
}

@media only screen and (max-width: 1000px) {
  .messaging {
    margin: 0 0 30px;
  }
}

@media only screen and (max-width: 860px) {
  .p-dialog-content > div,
  .headind_srch > div,
  .p-dialog-content > div > span.p-inputnumber,
  .headind_srch > div > span.p-inputnumber,
  .p-dialog-content > div > button,
  .headind_srch > div > button {
    float: none;
    width: 100%;
    margin-top: 5px;
  }

  .chat_ib span {
    margin: 6px;
  }

  .p-dialog-content {
    overflow: visible;
  }

  .chat_list,
  .headind_srch {
    padding: 5px;
  }
}
</style>
