<template>
  <div>
    <Toast position="top-right" />
    <ConfirmDialog />
    <div class="wrapper">
      <Panel
        :header="getString('First name')"
        class="panel"
      >
        <div class="content">
          {{ getName }}
        </div>
      </Panel>
      <Panel
        :header="getString('Last name')"
        class="panel"
      >
        <div class="content">
          {{ getSurname }}
        </div>
      </Panel>
      <Panel
        :header="getString('Username')"
        class="panel"
      >
        <div class="p-grid content">
          <InputText
            v-model="username"
            type="text"
            class="p-col p-inputtext-lg"
          />
          <h2 
            class="p-col"
            style="margin: 0; max-width: 100px; padding: 8px 0 8px 0;"
          >
            {{ "#" + getId.toLocaleString("en-US", {
              minimumIntegerDigits: 4,
              useGrouping: false
            })
            }}
          </h2>
          <Button
            class="p-button-lg"
            :label="getString('Save')"
            @click="changeSaveUsername()"
          />
        </div>
      </Panel>
      <Panel
        :header="getString('Index')"
        class="panel"
      >
        <div class="content">
          {{ getIndex }}
        </div>
      </Panel>
      <Panel
        :header="getString('Language')"
        class="panel"
      >
        <div class="content">
          <SelectButton
            v-model="language"
            :options="languageOptions"
          />
        </div>
      </Panel>
      <div
        v-if="!getRevealed"
        class="panel"
      >
        <Button
          class="p-button-lg"
          :label="getString('Reveal Yourself')"
          @click="RevealYourself(true)"
        />
      </div>
      <div
        v-else
        class="panel"
      >
        <Button
          class="p-button-lg p-button-outlined p-button-info"
          :label="getString('Already revealed')"
          @click="RevealYourself(false)"
        />
      </div>
      <div class="panel">
        <Button
          class="p-button-lg"
          :label="getString('Logout')"
          @click="logout"
        />
      </div>
    </div>
    <div
      v-if="getRole != 'User'"
      class="wrapper"
    >
      <Panel
        :header="getString(`Leader's section`)"
        class="panel"
      >
        <DataTable
          v-model:selection="selectedSubjects"
          style="padding-bottom: 1em;"
          :loading="getLoadingSubjects"
          :value="getSubjects"
          :paginator="true"
          paginatorTemplate="CurrentPageReport FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
          :rowsPerPageOptions="[5,10,15]"
          :currentPageReportTemplate="getString('currentPageReportTemplate')"
          :alwaysShowPaginator="false"
          :rows="5"
          dataKey="id"
          :rowClass="() => 'leader-row'"
        >
          <template #empty>
            {{ getString("No subjects to choose from") }}
          </template>
          <template #loading>
            {{ getString("Loading subjects, please wait...") }}
          </template>
          <Column
            selectionMode="multiple"
            headerStyle="width: 10%"
          />
          <Column
            field=""
            header-style="width: 45%"
          >
            <template 
              v-if="isMobile" 
              #header
            >
              <i class="pi pi-book" />
            </template>
            <template 
              v-else
              #header
            >
              {{ getString("Subjects") }}
            </template>
            <template 
              v-if="isMobile"
              #body="slotProps" 
            >
              <div class="subjects-column">
                {{ makeShorter(getLang(slotProps.data.name)) }}
              </div>
            </template>
            <template
              v-else
              #body="slotProps"
            >
              <div class="subjects-column">
                {{ getLang(slotProps.data.name) }}
              </div>
            </template>
          </Column>
          <Column
            field="submitted"
            header-style="width: 15%"
          >
            <template #header>
              <i class="pi pi-upload" />
            </template>
          </Column>
          <Column
            field="accepted"
            header-style="width: 15%"
          >
            <template #header>
              <i class="pi pi-check" />
            </template>
          </Column>
          <Column
            field="rejected"
            header-style="width: 15%; overflow: ellipsis "
          >
            <template #header>
              <i class="pi pi-times" />
            </template>
          </Column>
        </DataTable>
        <div style="min-width: 100%;">
          <Button
            class="p-button-lg"
            :label="getString('Calculate exchanges')"
            style="margin: 0 0 20px 0;"
            @click="EndExchangeWindow(selectedSubjects)"
          />
        </div>
        <Button
          class="p-button-lg"
          :label="getString('Send data to administration')"
          style="margin: 0 0 20px 0;"
          @click="SendToFaculty(selectedSubjects)"
        />
      </Panel>
    </div>
    <div
      v-if="getRole == 'Admin'"
      class="wrapper"
    >
      <Panel
        :header="getString(`Admin's section`)"
        class="panel"
      >
        <DataTable
          :loading="getLoadingLeaders"
          :value="getLeaders"
          :paginator="true"
          paginatorTemplate="CurrentPageReport FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
          :rowsPerPageOptions="[5,10,15]"
          :currentPageReportTemplate="getString('currentPageReportTemplate')"
          :alwaysShowPaginator="false"
          :rows="5"
          :rowClass="() => 'leader-row'"
        >
          <template #empty>
            {{ getString("No leaders") }}
          </template>
          <template #loading>
            {{ getString("Loading leaders, please wait...") }}
          </template>
          <Column
            :header="getString('List of leaders')"
            headerStyle="width: 50%; text-align: left;"
          >
            <template #body="slotProps">
              {{ slotProps.data.name }} {{ slotProps.data.surname }}
            </template>
          </Column>
          <Column
            field=""
            header=""
            headerStyle="width: 50%"
          >
            <template #body="slotProps">
              <div style="float: right;">
                <Button
                  class="p-button-danger"
                  :label="getString('Remove')"
                  @click="RemoveLeader(slotProps.data.studentNumber)"
                />
              </div>
            </template>
          </Column>
        </DataTable>
        <Panel
          :header="getString('Add leader')"
          class="panel"
        >
          <div class="content">
            <InputNumber
              v-model="leaderToAdd"
              mode="decimal"
              :useGrouping="false"
              :placeholder="getString('index')"
            />
            <Button
              class="p-button-success"
              :label="getString('Add')"
              @click="AddLeader(leaderToAdd)"
            />
          </div>
        </Panel>
        <DataTable
          :value="getAdmins"
          :paginator="true"
          paginatorTemplate="CurrentPageReport FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
          :rowsPerPageOptions="[5,10,15]"
          :currentPageReportTemplate="getString('currentPageReportTemplate')"
          :alwaysShowPaginator="false"
          :loading="getLoadingAdmins"
          :rows="5"
          :rowClass="() => 'admin-row'"
        >
          <template #loading>
            {{ getString("Loading admins, please wait...") }}
          </template>
          <template #empty>
            {{ getString("No admins") }}
          </template>
          <Column
            :header="getString('List of admins')"
            header-style="width: 50%; text-align: left;"
          >
            <template #body="slotProps">
              {{ slotProps.data.name }} {{ slotProps.data.surname }}
            </template>
          </Column>
          <Column
            field=""
            header=""
            header-style="width: 50%"
          >
            <template #body="slotProps">
              <div style="float: right;">
                <Button
                  class="p-button-danger"
                  :label="getString('Remove')"
                  @click="RemoveAdmin(slotProps.data.studentNumber)"
                />
              </div>
            </template>
          </Column>
        </DataTable>
        <Panel
          :header="getString('Add admin')"
          class="panel"
        >
          <div class="content">
            <InputNumber
              v-model="adminToAdd"
              mode="decimal"
              :use-grouping="false"
              :placeholder="getString('index')"
            />
            <Button
              class="p-button-success"
              :label="getString('Add')"
              @click="AddAdmin(adminToAdd)"
            />
          </div>
        </Panel>
        <Button
          class="p-button-lg"
          :label="getString('Start new semester')"
          style="margin: 0 0 20px 0;"
          @click="StartNewSemester()"
        />
      </Panel>
    </div>
  </div>
</template>

<script>
import { mapGetters } from "vuex";
import { mapActions } from "vuex";
import Panel from "primevue/panel";
import Button from "primevue/button";
import SelectButton from "primevue/selectbutton";
import InputNumber from "primevue/inputnumber";
import InputText from "primevue/inputtext";
import DataTable from "primevue/datatable";
import Column from "primevue/column";
import Toast from "primevue/toast";
import ConfirmDialog from "primevue/confirmdialog";

export default {
  name: "Settings",
  components: {
    Panel,
    Button,
    SelectButton,
    InputNumber,
    DataTable,
    Column,
    Toast,
    ConfirmDialog,
    InputText
  },
  data() {
    return {
      adminToAdd: null,
      leaderToAdd: null,
      languageOptions: [],
      selectedSubjects: [],
      isMobile: screen.width <= 620
    };
  },
  computed: {
    ...mapGetters([
      "getLoadingSubjects",
      "getSubjects",
      "getAdmins",
      "getLeaders",
      "getLanguage",
      "getRole",
      "getId",
      "getUsername",
      "getSurname",
      "getName",
      "getIndex",
      "getRevealed",
      "getLoadingLeaders",
      "getLoadingAdmins",
      "getLang",
      "getString"
    ]),
    language: {
      get() {
        return this.getString(this.getLanguage);
      },
      set(value) {
        if (value == "Polish" || value == "Polski") {
          this.changeLanguage("pl").then(
            () =>
              (this.languageOptions = [
                this.getString("pl"),
                this.getString("en")
              ])
          );
        } else {
          this.changeLanguage("en").then(
            () =>
              (this.languageOptions = [
                this.getString("pl"),
                this.getString("en")
              ])
          );
        }
      }
    },
    username: {
      get() {
        return this.getUsername;
      },
      set(value) {
        this.changeUsername(value);
      }
    }
  },
  created() {
    this.fetchUserInfo().then(() => {
      this.username = this.getUsername;
      this.languageOptions = [this.getString("pl"), this.getString("en")];
    });
    this.isMobile = screen.width <= 620;
    window.addEventListener("resize", () => this.isMobile = screen.width <= 620);
  },
  methods: {
    ...mapActions([
      "fetchUserInfo",
      "sendToFaculty",
      "removeAdmin",
      "addAdmin",
      "removeLeader",
      "addLeader",
      "changeLanguage",
      "changeUsername",
      "saveUsername",
      "logout",
      "revealYourself",
      "checkSubject",
      "unCheckSubject",
      "endExchangeWindow",
      "startNewSemester",
    ]),
    RevealYourself(reveal) {
      if(reveal) {
        this.$confirm.require({
          message: this.getString(
            "Are you sure you want your name to be visible to all users in this group? This cannot be reversed"
          ),
          header: this.getString("Confirmation"),
          icon: "pi pi-exclamation-triangle",
          accept: () => {
            this.revealYourself(true);
          },
          acceptLabel: this.getString("Yes"),
          rejectLabel: this.getString("No"),
        });
      } else {
        this.revealYourself(false);
      }
    },
    EndExchangeWindow(ids) {
      this.$confirm.require({
        message: this.getString(
          "Are you sure you want your calculate exchanges for selected subjects?"
        ),
        header: this.getString("Confirmation"),
        icon: "pi pi-exclamation-triangle",
        accept: () => {
          var id = [];
          for (var key in ids) {
            id.push(ids[key].id);
          }
          this.endExchangeWindow(id);
        },
        acceptLabel: this.getString("Yes"),
        rejectLabel: this.getString("No"),
      });
    },
    StartNewSemester() {
      this.$confirm.require({
        message: this.getString(
          "Are you sure you want start new semster? This action will delete all current data about exchanges and timetables"
        ),
        header: this.getString("Confirmation"),
        icon: "pi pi-exclamation-triangle",
        accept: () => {
          this.startNewSemester(true);
        },
        acceptLabel: this.getString("Yes"),
        rejectLabel: this.getString("No"),
      });
    },
    RemoveAdmin(id) {
      if (this.getAdmins.length <= 1) {
        this.$toast.add({
          severity: "error",
          summary: this.getString("Cannot remove last admin"),
          life: 3000
        });
      } else {
        this.$confirm.require({
          message: String.format(
            this.getString(
              "Are you sure you want to take away admin privileges from user with ID: {0}?"
            ),
            id
          ),
          header: this.getString("Confirmation"),
          icon: "pi pi-exclamation-triangle",
          accept: () => {
            this.removeAdmin(id);
          },
          acceptLabel: this.getString("Yes"),
          rejectLabel: this.getString("No"),
        });
      }
    },
    AddAdmin(id) {
      if (id == null || !Number.isInteger(Number(id))) {
        this.$toast.add({
          severity: "error",
          summary: this.getString("Admin ID must be a number"),
          life: 3000
        });
      } else {
        this.$confirm.require({
          message: String.format(
            this.getString(
              "Are you sure you want to grant admin privileges to user with ID: {0}?"
            ),
            id
          ),
          header: this.getString("Confirmation"),
          icon: "pi pi-exclamation-triangle",
          accept: () => {
            this.addAdmin(id);
            this.adminToAdd = "";
          },
          acceptLabel: this.getString("Yes"),
          rejectLabel: this.getString("No"),
        });
      }
    },
    RemoveLeader(id) {
      this.$confirm.require({
        message: String.format(
          this.getString(
            "Are you sure you want to take away leader privileges from user with ID: {0}?"
          ),
          id
        ),
        header: this.getString("Confirmation"),
        icon: "pi pi-exclamation-triangle",
        accept: () => {
          this.removeLeader(id);
        },
        acceptLabel: this.getString("Yes"),
        rejectLabel: this.getString("No"),
      });
    },
    AddLeader(id) {
      if (id == null || !Number.isInteger(Number(id))) {
        this.$toast.add({
          severity: "error",
          summary: this.getString("Leader ID must be a number"),
          life: 3000
        });
      } else {
        this.$confirm.require({
          message: String.format(
            this.getString(
              "Are you sure you want to grant leader privileges to user with ID: {0}?"
            ),
            id
          ),
          header: this.getString("Confirmation"),
          icon: "pi pi-exclamation-triangle",
          accept: () => {
            this.addLeader(id);
            this.leaderToAdd = "";
          },
          acceptLabel: this.getString("Yes"),
          rejectLabel: this.getString("No"),
        });
      }
    },
    SendToFaculty(ids) {
      let message = this.getString(
        "Are you sure you want send current state to faculty administrator?"
      );
      this.$confirm.require({
        message: message,
        header: this.getString("Confirmation"),
        icon: "pi pi-exclamation-triangle",
        accept: () => {
          var id = [];
          for (var key in ids) {
            id.push(ids[key].id);
          }
          this.sendToFaculty(id);
        },
        acceptLabel: this.getString("Yes"),
        rejectLabel: this.getString("No"),
      });
    },
    makeShorter(text) {
      var out = "";
      text.split(" ").forEach(element => {
        out += element[0];
      });
      return out.toUpperCase();
    },
    changeSaveUsername() {
      if(/^[A-Za-z0-9]+$/i.test(this.username)) {
        this.saveUsername();
      } else {
        this.$toast.add({
          severity: "error",
          summary: this.getString("Username cannot contain special characters"),
          life: 3000
        });
      }
    }
  }
};
</script>

<style scoped>
.wrapper {
  margin: 0 calc(50% - 304px) 30px;
  box-shadow: 0 35px 50px rgba(0, 0, 0, 0.2);
  padding: 1em;
}

@media only screen and (max-width: 630px) {
  .wrapper {
    margin: 0 0 30px;
  }
}

@media only screen and (max-width: 420px) {
  .wrapper {
    margin: 0 0 30px;
  }
}

.panel {
  position: relative;
  margin-bottom: 1em;
}

.content {
  text-align: left;
}

.content > button {
  float: right;
}

@media only screen and (max-width: 420px) {
  .content > button {
    float: none;
    width: 100%;
    margin-top: 5px;
  }

  .content > input {
    float: none;
    width: 100%;
  }

  .content > span.p-inputnumber {
    float: none;
    width: 100%;
  }
}
</style>
