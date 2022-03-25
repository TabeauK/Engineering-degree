<template>
  <div>
    <DataTable
      v-model:expandedRows="expandedRows"
      :value="getExchanges"
      paginatorTemplate="CurrentPageReport FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
      :rowsPerPageOptions="[5,10,15]"
      :currentPageReportTemplate="getString('currentPageReportTemplate')"
      :alwaysShowPaginator="false"
      :paginator="true"
      :rows="5"
      :loading="getLoadingExchanges"
      :rowClass="() => 'exchange-row'"
    >
      <template #empty>
        {{ getString("Currently, you don't have any active exchanges") }}
      </template>
      <template #loading>
        {{ getString("Loading exchanges, please wait...") }}
      </template>
      <template 
        v-if="!isMobile" 
        #expansion="slotProps"
      >
        <TreeTable
          class="tree"
          :value="slotProps.data.relations"
        >
          <Column
            field=""
            header=""
            headerStyle="width: 10%"
          >
            <template #body="slot">
              <div class="column-from">
                {{
                  getString(slot.node.type + " with")
                }}
              </div>
            </template>
          </Column>
          <Column
            field="groupTo"
            header=""
            headerStyle="width: 30%; text-align: center;"
          >
            <template #body="slot">
              <div class="column-from">
                {{
                  getLang(slot.node.relationWith[0].groupFrom.displayName) +
                    " - " +
                    getString("Group") +
                    " " +
                    slot.node.relationWith[0].groupFrom.groupNumber
                }}
              </div>
            </template>
          </Column>
          <Column
            field=""
            header=""
            headerStyle="width: 10%"
          >
            <template #body="">
              <div class="column-arrow">
                &#10230;
              </div>
            </template>
          </Column>
          <Column
            field=""
            header=""
            headerStyle="width: 30%; text-align: center;"
          >
            <template #body="slot">
              <div class="column-to">
                {{
                  getLang(slot.node.relationWith[0].groupTo.displayName) +
                    " - " +
                    getString("Group") +
                    " " +
                    slot.node.relationWith[0].groupTo.groupNumber
                }}
              </div>
            </template>
          </Column>
          <Column 
            headerStyle="width: 20%; text-align:right;"
          >
            <template #body="slot">
              <div
                class="exchange-button"
              >
                <Button
                  class="p-button-danger"
                  :label="getString('Cancel relation')"
                  @click="removeRelation(slot.node.id)"
                />
              </div>
            </template>
          </Column>
        </TreeTable>
      </template>
      <template 
        v-else
        #expansion="slotProps"
      >
        <TreeTable
          class="tree"
          :value="slotProps.data.relations"
        >
          <Column
            headerStyle="width: 10%"
          >
            <template #body="slot">
              <div class="column-from">
                <i 
                  :class="
                    slot.node.type == 'And'
                      ? 'pi pi-check-circle'
                      : 'pi pi-times-circle'"
                  style="fontsize: 2rem;"
                />
              </div>
            </template>
          </Column>
          <Column
            headerStyle="width: 30%; text-align: center;"
          >
            <template #body="slot">
              <div class="column-from">
                {{
                  makeShorter(getLang(slot.node.relationWith[0].groupFrom.displayName)) +
                    " " +
                    makeShorter(getString("Group")) + 
                    slot.node.relationWith[0].groupFrom.groupNumber
                }}
              </div>
            </template>
          </Column>
          <Column
            headerStyle="width: 10%"
          >
            <template #body="">
              <div class="column-arrow">
                &#10230;
              </div>
            </template>
          </Column>
          <Column
            headerStyle="width: 30%; text-align: center;"
          >
            <template #body="slot">
              <div class="column-to">
                {{
                  makeShorter(getLang(slot.node.relationWith[0].groupTo.displayName)) +
                    " " +
                    makeShorter(getString("Group")) +
                    slot.node.relationWith[0].groupTo.groupNumber
                }}
              </div>
            </template>
          </Column>
          <Column 
            headerStyle="width: 20%; text-align:right;"
          >
            <template #body="slot">
              <div
                class="exchange-button"
              >
                <Button
                  class="p-button-danger"
                  :label="getString('Cancel relation')"
                  @click="removeRelation(slot.node.id)"
                />
              </div>
            </template>
          </Column>
        </TreeTable>
      </template>
      <Column
        :expander="true"
        headerStyle="width: 5%"
      />
      <Column
        field=""
        :header="getString('From')"
        class="subject"
        :headerStyle="isMobile ? 'width: 20%; text-align: center;' : 'width: 20%; text-align: center;'"
      >
        <template #body="slotProps">
          <div
            v-if="!isMobile"
            class="column-from"
          >
            {{
              getLang(slotProps.data.groupFrom.displayName) +
                " - " +
                getString("Group") +
                " " +
                slotProps.data.groupFrom.groupNumber
            }}
          </div>
          <div
            v-else
            class="column-from"
          >
            {{
              makeShorter(getLang(slotProps.data.groupFrom.displayName)) +
                " " +
                makeShorter(getString("Group")) +
                slotProps.data.groupFrom.groupNumber
            }}
          </div>
        </template>
      </Column>
      <Column
        field=""
        header=""
        headerStyle="width: 15%"
      >
        <template #body="">
          <div class="column-arrow">
            &#10230;
          </div>
        </template>
      </Column>
      <Column
        field=""
        :header="getString('To')"
        :headerStyle="isMobile ? 'width: 20%; text-align: center;' : 'width: 20%; text-align: center;'"
      >
        <template #body="slotProps">
          <div
            v-if="!isMobile"
            class="column-from"
          >
            {{
              getLang(slotProps.data.groupFrom.displayName) +
                " - " +
                getString("Group") +
                " " +
                slotProps.data.groupFrom.groupNumber
            }}
          </div>
          <div
            v-else
            class="column-from"
          >
            {{
              makeShorter(getLang(slotProps.data.groupFrom.displayName)) +
                " " +
                makeShorter(getString("Group")) +
                slotProps.data.groupFrom.groupNumber
            }}
          </div>
        </template>
      </Column>
      <Column
        field=""
        header=""
        :headerStyle="isMobile ? 'width: 20%;' : 'width: 15%;'"
      >
        <template #body="slotProps">
          <div
            class="exchange-button"
          >
            <Button
              class="p-button-success"
              :label="getString('Add dependency relation')"
              @click="openDependencyModal(slotProps.data.id)"
            />
          </div>
          <Dialog
            v-model:visible="DependencyModal[slotProps.data.id]"
            :header="isMobile ?
              getString('Add dependency relation to ') +
              makeShorter(getLang(slotProps.data.groupFrom.displayName)) +
              ' ' +
              makeShorter(getString('Group')) +
              slotProps.data.groupFrom.groupNumber +
              ' &#10230; ' +
              makeShorter(getLang(slotProps.data.groupTo.displayName)) +
              ' ' +
              makeShorter(getString('Group')) +
              slotProps.data.groupTo.groupNumber
              :
              getString('Add dependency relation to ') +
              getLang(slotProps.data.groupFrom.displayName) +
              ' - ' +
              getString('Group') +
              ' ' +
              slotProps.data.groupFrom.groupNumber +
              ' &#10230; ' +
              getLang(slotProps.data.groupTo.displayName) +
              ' - ' +
              getString('Group') +
              ' ' +
              slotProps.data.groupTo.groupNumber"
            :modal="true"
          >
            <div>
              <DataTable
                :value="getExchanges.filter(x=> x.id != slotProps.data.id && !(slotProps.data.relations.some(y => y.relationWith.some(z => z.id == x.id))))"
                paginatorTemplate="CurrentPageReport FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
                :rowsPerPageOptions="[5,10,15]"
                :currentPageReportTemplate="getString('currentPageReportTemplate')"
                :alwaysShowPaginator="false"
                :paginator="true" 
                :rows="5"
                :rowClass="() => ' exchange-row exchange-row-modal'"
                @row-click="
                  e => {
                    addRelation({
                      fromId: slotProps.data.id,
                      toId: e.data.id,
                      type: 'And'
                    });
                    closeDependencyModal(slotProps.data.id);
                  }"
              >
                <template #empty>
                  {{ getString("No exchanges to choose from") }}
                </template>
                <Column
                  field=""
                  :header="getString('From')"
                  headerStyle="width: 25%; text-align: center;"
                >
                  <template #body="slot">
                    <div class="column-from">
                      {{
                        getLang(slot.data.groupFrom.displayName) +
                          " - " +
                          getString("Group") +
                          " " +
                          slot.data.groupFrom.groupNumber
                      }}
                    </div>
                  </template>
                </Column>
                <Column
                  field=""
                  header=""
                  headerStyle="width: 10%"
                >
                  <template #body="">
                    <div class="column-arrow">
                      &#10230;
                    </div>
                  </template>
                </Column>
                <Column
                  field=""
                  :header="getString('To')"
                  headerStyle="width: 25%; text-align: center;"
                >
                  <template #body="slot">
                    <div class="column-to">
                      {{
                        getLang(slot.data.groupTo.displayName) +
                          " - " +
                          getString("Group") +
                          " " +
                          slot.data.groupTo.groupNumber
                      }}
                    </div>
                  </template>
                </Column>
              </DataTable>
              <div class="close-button">
                <Button
                  class="p-button-warning"
                  :label="getString('Close')"
                  @click="closeDependencyModal(slotProps.data.id)"
                />
              </div>
            </div>
          </Dialog>
        </template>
      </Column>
      <Column
        field=""
        header=""
        :headerStyle="isMobile ? 'width: 20%;' : 'width: 15%;'"
      >
        <template #body="slotProps">
          <div
            class="exchange-button"
          >
            <Button
              class="p-button-danger"
              :label="getString('Add excluding relation')"
              @click="openExclusionModal(slotProps.data.id)"
            >
              <div class="p-button-label"> 
                {{ getString('Add excluding relation') }}
              </div>
            </Button>
          </div>
          <Dialog
            v-model:visible="ExclusionModal[slotProps.data.id]"
            :header="isMobile ?
              getString('Add excluding relation to ') +
              makeShorter(getLang(slotProps.data.groupFrom.displayName)) +
              ' ' +
              makeShorter(getString('Group')) +
              slotProps.data.groupFrom.groupNumber +
              ' &#10230; ' +
              makeShorter(getLang(slotProps.data.groupTo.displayName)) +
              ' ' +
              makeShorter(getString('Group')) +
              slotProps.data.groupTo.groupNumber
              :
              getString('Add excluding relation to ') +
              getLang(slotProps.data.groupFrom.displayName) +
              ' - ' +
              getString('Group') +
              ' ' +
              slotProps.data.groupFrom.groupNumber +
              ' &#10230; ' +
              getLang(slotProps.data.groupTo.displayName) +
              ' - ' +
              getString('Group') +
              ' ' +
              slotProps.data.groupTo.groupNumber"
            :modal="true"
          >
            <div>
              <DataTable
                :value="getExchanges.filter(x=> x.id != slotProps.data.id && !(slotProps.data.relations.some(y => y.relationWith.some(z => z.id == x.id))))"
                paginatorTemplate="CurrentPageReport FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
                :rowsPerPageOptions="[5,10,15]"
                :currentPageReportTemplate="getString('currentPageReportTemplate')"
                :alwaysShowPaginator="false"
                :paginator="true" 
                :rows="5"
                :rowClass="() => 'exchange-row exchange-row-modal'"
                @row-click="
                  e => {
                    addRelation({
                      fromId: slotProps.data.id,
                      toId: e.data.id,
                      type: 'Xor'
                    });
                    closeExclusionModal(slotProps.data.id);
                  }"
              >
                <template #empty>
                  {{ getString("No exchanges to choose from") }}
                </template>
                <Column
                  field=""
                  :header="getString('From')"
                  headerStyle="width: 25%; text-align: center;"
                >
                  <template #body="slot">
                    <div class="column-from">
                      {{
                        getLang(slot.data.groupFrom.displayName) +
                          " - " +
                          getString("Group") +
                          " " +
                          slot.data.groupFrom.groupNumber
                      }}
                    </div>
                  </template>
                </Column>
                <Column
                  field=""
                  header=""
                  headerStyle="width: 10%"
                >
                  <template #body="">
                    <div class="column-arrow">
                      &#10230;
                    </div>
                  </template>
                </Column>
                <Column
                  field=""
                  :header="getString('To')"
                  headerStyle="width: 25%; text-align: center;"
                >
                  <template #body="slot">
                    <div class="column-to">
                      {{
                        getLang(slot.data.groupTo.displayName) +
                          " - " +
                          getString("Group") +
                          " " +
                          slot.data.groupTo.groupNumber
                      }}
                    </div>
                  </template>
                </Column>
              </DataTable>
              <div class="close-button">
                <Button
                  class="p-button-warning"
                  :label="getString('Close')"
                  @click="closeExclusionModal(slotProps.data.id)"
                />
              </div>
            </div>
          </Dialog>
        </template>
      </Column>
    </DataTable>
  </div>
</template>

<script>
import { mapGetters } from "vuex";
import { mapActions } from "vuex";
import TreeTable from "primevue/treetable";
import DataTable from "primevue/datatable";
import Column from "primevue/column";
import Dialog from "primevue/dialog";
import Button from "primevue/button";
//import ScrollPanel from "primevue/scrollpanel";

export default {
  name: "Exchanges",
  components: { DataTable, Column, Dialog, Button, TreeTable },
  data: () => ({
    DependencyModal: Array(),
    ExclusionModal: Array(),
    isMobile: screen.width <= 820,
    expandedRows: [],
  }),
  computed: mapGetters([
    "getExchanges",
    "getLang",
    "getString",
    "getLoadingExchanges"
  ]),
  created() {
    this.fetchExchanges();
    this.getExchanges.forEach(element => {
      this.DependencyModal[element.id] = false;
      this.ExclusionModal[element.id] = false;
    });
    this.isMobile = screen.width <= 820;
    window.addEventListener("resize", () => this.isMobile = screen.width <= 820);
  },
  methods: {
    ...mapActions(["fetchExchanges", "addRelation", "removeRelation"]),
    openDependencyModal(id) {
      if (
        this.DependencyModal.every(x => x == false) &&
        this.ExclusionModal.every(x => x == false)
      ) {
        this.DependencyModal[id] = true;
      }
    },
    closeDependencyModal(id) {
      this.DependencyModal[id] = false;
      this.$forceUpdate();
    },
    openExclusionModal(id) {
      if (
        this.DependencyModal.every(x => x == false) &&
        this.ExclusionModal.every(x => x == false)
      ) {
        this.ExclusionModal[id] = true;
      }
    },
    closeExclusionModal(id) {
      this.ExclusionModal[id] = false;
      this.$forceUpdate();
    },
    makeShorter(text) {
      var out = "";
      text.split(" ").forEach(element => {
        out += element[0];
      });
      return out.toUpperCase();
    }
  }
};
</script>

<style>
.exchange-text {
  min-height: 100px !important;
}

.exchange-text > button {
  height: 100px !important;
  width: 80%;
  text-align: center;
  align-content: center;
  padding: 0 !important;
  word-wrap: break-word;
}

.exchange-text > button > i {
  font-size: 2em;
  text-align: center;
  align-content: center;
  position: relative;
}

.exchange-button > button {
  word-wrap: break-word;
  width: 80%;
  height: 100%;
  min-height: 60px;
}

div.column-arrow {
  width: 100%;
  font-size: 40px;
  text-align: center;
  height: 100%;
  padding: 0;
}

tr.exchange-row > td > div.exchange-text,
tr.exchange-row > td > div.exchange-button,
.p-treetable-tbody > tr > td > div.exchange-button {
  width: 100%;
  align-content: center;
  text-align: center;
  min-height: 100%;
}

div.column-to,
div.column-from {
  width: 100%;
  text-align: center;
  height: 100%;
  padding: 16px;
}

tr.exchange-row:nth-of-type(even) {
  background-color: lightgray;
}

.p-datatable-row-expansion > td {
  padding: 0 !important;
}

.p-treetable-thead > tr > th {
  padding: 0 !important;
}


tr.exchange-row > td,
.p-treetable-tbody > tr > td {
  padding: 0 !important;
}

tr.exchange-row-modal:hover {
  border: 3px solid blue;
}

div.close-button {
  float: right;
}

.orders-subtable {
  margin-left: 20px;
  margin-right: 5px;
  padding-right: 20px;
  height: 400px;
}

.orders-subtable.p-scrollpanel-wrapper {
  border-right: 9px solid var(--surface-b);
}

.orders-subtable.p-scrollpanel-bar {
  background-color: var(--primary-color);
  opacity: 1;
  transition: background-color .2s;
}

.orders-subtable.p-scrollpanel-bar:hover {
  background-color: #007ad9;
}

@media only screen and (max-width: 820px) {
  .close-button {
    width: 100%;
  }

  .close-button > button {
    width: 100%;
  }

  .exchange-button > button {
    padding: 0 !important;
  }
}
</style>
