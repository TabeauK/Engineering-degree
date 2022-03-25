<template>
  <div>
    <ConfirmDialog />
    <DataView
      :value="getGroups"
      paginatorTemplate="CurrentPageReport FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
      :rowsPerPageOptions="[6,9,12]"
      :currentPageReportTemplate="getString('currentPageReportTemplate')"
      :alwaysShowPaginator="false"
      :paginator="true"
      :rows="6"
      :loading="getLoadingTeams"
      :layout="'grid'"
      class="teams"
    >
      <template #loading>
        {{ getString("Loading groups, please wait...") }}
      </template>
      <template #empty>
        <div style="padding: 1em;">
          {{ getString("Currently, you don't have any teams") }}
        </div>
      </template>
      <template #grid="slotProps">
        <div class="p-col-12 p-md-4">
          <div class="product-grid-item card">
            <div class="product-grid-item-content">
              <DataTable
                :value="slotProps.data.students"
                :rows="5"
                :rowClass="() => 'group-row'"
                :scrollable="true"
                scrollHeight="300px"
                class="p-datatable-sm"
              >
                <template #header>
                  <div class="p-d-flex p-jc-between p-ai-center">
                    <h3 class="p-m-0">
                      {{ getLang(slotProps.data.name) }}
                    </h3>
                    <span class="p-input-icon-left" />
                  </div>
                </template>
                <Column
                  field=""
                  headerStyle="width: 70%; text-align: center; max-height: 1px; padding:0"
                >
                  <template 
                    v-if="!slotProps.data.students.some(x => x.id == getId && x.status == 'Pending')"
                    #header 
                  >
                    <div
                      style="width: 100%;"
                      class="p-fluid"
                    >
                      <AutoComplete
                        v-model="userToAdd[slotProps.data.id]"
                        :suggestions="suggestionsForTeam[slotProps.data.id]"
                        field="name"
                        :placeholder="getString('Type in a username...')"
                        @complete="{ selected[slotProps.data.id] = false; fetchSuggestionsForT({ prefix: userToAdd[slotProps.data.id], teamId: slotProps.data.id }); }"
                        @item-select=" selected[slotProps.data.id] = true"
                        @item-unselect=" selected[slotProps.data.id] = false"
                      />
                    </div>
                  </template>
                  <template
                    #body="props"
                    style="padding: 0%;"
                  >
                    <div
                      v-if="props.data.status == 'Pending'"
                      class="pending column-students"
                    >
                      <i
                        class="pi pi-circle-on"
                        style="size: 1rem;"
                      />
                      {{ props.data.name + " (" + getString("Pending") + ")" }}
                    </div>
                    <div
                      v-else-if="props.data.id == getId"
                      class="me column-students"
                    >
                      <i
                        class="pi pi-circle-on"
                        style="size: 1rem;"
                      />
                      {{ props.data.name + " (" + getString("you") + ")" }}
                    </div>
                    <div
                      v-else
                      class="column-students"
                    >
                      <i
                        class="pi pi-circle-on"
                        style="size: 1rem;"
                      />
                      {{ props.data.name }}
                    </div>
                  </template>
                </Column>
                <Column
                  field=""
                  headerStyle="width: 30%; max-height: 1px; padding:0"
                >
                  <template #header>
                    <div
                      class="buttons"
                    >
                      <Button
                        v-if="selected[slotProps.data.id]"
                        :label="getString('Invite')"
                        class="p-button-success"
                        @click="inviteUser(slotProps.data.id)"
                      />
                      <Button
                        v-else
                        :label="getString('Invite')"
                        class="p-button-success"
                        disabled="disabled"
                        @click="inviteUser(slotProps.data.id)"
                      />
                    </div>
                  </template>
                  <template
                    #body="props"
                    style="padding: 0%;"
                  >
                    <div 
                      v-if="!slotProps.data.students.some(x => x.inviteId != 0 && x.id == getId) || getId == props.data.id"
                      class="buttons"
                    >
                      <Button
                        type="button"
                        :label="getText(props.data, getId)"
                        class="p-button-raised p-button-text"
                        @click="toggleModal(slotProps.data, props.data)"
                      />
                    </div>
                  </template>
                </Column>
              </DataTable>
            </div>
            <div class="product-grid-item-bottom" />
          </div>
        </div>
      </template>
    </DataView>
    <Dialog
      v-model:visible="isModalVisible"
      :style="{width: '450px'}"
      header=""
      :modal="true"
    >
      <div class="confirmation-content">
        <span v-if="modalData.props.id == getId && modalData.props.status == 'Accepted'">{{ String.format(
          getString("Do you want to leave {0} subject's team?"), 
          getLang(modalData.slotProps.name)) 
        }}
        </span>
        <span v-if="modalData.props.id == getId && modalData.props.status == 'Pending'">{{ String.format(
          getString("You have been invited to the team in course {0} by {1}"), 
          getLang(modalData.slotProps.name), 
          modalData.props.inviting) 
        }}
        </span>
        <span v-if="modalData.props.id != getId && modalData.props.status == 'Pending'">{{ String.format(
          getString("Are you sure you want to stop inviting"),
          modalData.props.name,
          getLang(modalData.slotProps.name)) 
        }}
        </span>
        <span v-if="modalData.props.id != getId && modalData.props.status == 'Accepted'">{{ String.format(
          getString("Are you sure you want to remove user"),
          modalData.props.name,
          getLang(modalData.slotProps.name)) 
        }}
        </span>
      </div>
      <template #footer>
        <Button
          v-if="modalData.props.id == getId && modalData.props.status == 'Accepted'"
          :label="getString('Leave')"
          class="p-button-warning"
          @click="removeUser({ id: modalData.slotProps.id, user: modalData.props.id }); isModalVisible=false;"
        />
        <Button
          v-if="modalData.props.id == getId && modalData.props.status == 'Pending'"
          :label="getString('Accept')"
          class="p-button-success"
          @click="acceptTeamInvite(modalData.props.inviteId); isModalVisible=false;"
        />
        <Button
          v-if="modalData.props.id == getId && modalData.props.status == 'Pending'"
          :label="getString('Reject')"
          class="p-button-danger"
          @click="rejectTeamInvite(modalData.props.inviteId); isModalVisible=false;"
        />
        <Button
          v-if="modalData.props.id != getId && modalData.props.status == 'Pending'"
          class="p-button-warning"
          :label="getString('Stop inviting user')"
          @click="stopInvitingFromGroup(modalData.props.inviteId); isModalVisible=false;"
        />
        <Button
          v-if="modalData.props.id != getId && modalData.props.status == 'Accepted'"
          :label="getString('Remove user')"
          class="p-button-warning"
          @click="removeUser({ id: modalData.slotProps.id, user: modalData.props.id }); isModalVisible=false;"
        />
      </template>
    </Dialog>
  </div>
</template>

<script>
import Dialog from "primevue/dialog";
import DataTable from "primevue/datatable";
import Button from "primevue/button";
import Column from "primevue/column";
import DataView from "primevue/dataview";
import ConfirmDialog from "primevue/confirmdialog";
import { mapGetters } from "vuex";
import { mapActions } from "vuex";
import AutoComplete from "primevue/autocomplete";

export default {
  name: "Groups",
  components: { DataView, DataTable, Button, Column, ConfirmDialog, Dialog, AutoComplete },
  data() {
    return {
      userToAdd: {},
      selected: {},
      isModalVisible: false,
      modalData: {},
      suggestionsForTeam: {},
    };
  },
  computed: mapGetters(["getGroups", "getId", "getString", "getLang", "getSuggestionsForTeam"]),
  created() {
    this.fetchGroups();
  },
  methods: {
    ...mapActions([
      "fetchGroups",
      "removeUser",
      "stopInvitingFromGroup",
      "getLoadingTeams",
      "acceptTeamInvite",
      "rejectTeamInvite",
      "fetchSuggestionsForTeam",
      "inviteUserToTeam",
    ]),
    toggleModal(slotProps, props) {
      this.isModalVisible = true;
      this.modalData = {slotProps, props};
    },
    fetchSuggestionsForT(data) {
      this.fetchSuggestionsForTeam(data).then(() => { this.suggestionsForTeam[data.teamId] = []; this.suggestionsForTeam[data.teamId] = this.getSuggestionsForTeam[data.teamId]; });
    },
    getText(props, Id) {
      if(props.id == Id && props.status == "Accepted") {
        return this.getString("Leave");
      } else if(props.id == Id && props.status == "Pending") {
        return this.getString("Decide");
      } else if(props.id != Id && props.status == "Pending") {
        return this.getString("Stop inviting user");
      } else if(props.id != Id && props.status == "Accepted") {
        return this.getString("Remove user");
      } else {
        return null;
      }
    },
    inviteUser(teamId) {
      if (!this.selected[teamId] || this.userToAdd[teamId] == null || this.userToAdd[teamId] == undefined || this.userToAdd[teamId].value == null || this.userToAdd[teamId].value == undefined ) {
        this.$toast.add({
          severity: "error",
          summary: this.getString("User must be selected from list"),
          life: 3000
        });
      } else {
        this.inviteUserToTeam({id : this.userToAdd[teamId].value, teamId }).then(
          () => (this.userToAdd[teamId] = null)
        );
      }
    },
  }
};
</script>

<style scoped>
tr.group-row > td > div.column-students {
  width: 100%;
  text-align: left;
  height: 100%;
  font-weight: 700;
}

.card {
  -webkit-box-shadow:
    0 2px 1px -1px rgba(0, 0, 0, 0.2),
    0 1px 1px 0 rgba(0, 0, 0, 0.14),
    0 1px 3px 0 rgba(0, 0, 0, 0.12);
  box-shadow:
    0 2px 1px -1px rgba(0, 0, 0, 0.2),
    0 1px 1px 0 rgba(0, 0, 0, 0.14),
    0 1px 3px 0 rgba(0, 0, 0, 0.12);
  border-radius: 4px;
  margin-bottom: 2rem;
}

.buttons {
  word-wrap: break-word;
  align-content: center;
  text-align: center;
  min-height: 100%;
  display: flex;
  justify-content: space-around;
  width: 100%;
}

.buttons > button {
  width: 100%;
}

.p-autocomplete .p-component .p-inputwrapper {
  width: 100% !important;
}

.product-grid-item {
  margin: 0.5rem;
  border: 1px solid #dee2e6;
}

div.remove-button {
  float: right;
}

.pending {
  color: lightgray;
}

.me {
  color: blueviolet;
}
</style>
